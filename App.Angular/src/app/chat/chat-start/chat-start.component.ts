import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, NgZone, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatMessage } from '../../models/test/ChatMessage.model';
import { ConversationService } from '../../services/conversation.service';
import { GetmeService } from '../../services/getme.service';
import { HubreceiverService } from '../../services/hubreceiver.service';
import { SendmessageService } from '../../services/sendmessage.service';

@Component({
  selector: 'app-chat-start',
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-start.component.html',
  styleUrl: './chat-start.component.css'
})
export class ChatStartComponent implements AfterViewInit, OnInit, OnChanges {

  @Input() conversationData: any[] = [];
  @Input() aliciNumara: string;
  @Input() aliciUserId: string;
  @Input() aliciName: string;

  currentNumber: string;
  currentUserId: string;
  currenUserName: string;

  userInputMessage: string = '';
  statusMessage: string = '';
  typingTimeout: any;

  @ViewChild('chatContainer') chatContainer: ElementRef;

  constructor(
    private hubService: HubreceiverService,
    private sendMessageService: SendmessageService,
    private conversationService: ConversationService,
    private ngZone: NgZone,
    private getmeService: GetmeService
  ) {}

  async ngOnInit() {
    await this.getMe();

    console.log('📞 aliciNumara:', this.aliciNumara);
    console.log('📱 currentNumber:', this.currentNumber);

    // Hub bağlantısı başlat (tek sefer)
    this.hubService.startConnection();

    // ✅ Hub bağlantısı hazır olduğunda eventleri dinle
    const waitForHub = setInterval(() => {
      if (this.hubService.hubConnection && this.hubService.hubConnection.state === 'Connected') {
        console.log('✅ Hub bağlantısı aktif, eventler dinleniyor...');

        // 🔹 Yeni mesaj geldiğinde çalışır
        this.hubService.hubConnection.off('ReceiveMessageAsync');

        this.hubService.hubConnection.on('ReceiveMessageAsync', (chatMessage: ChatMessage) => {
          this.ngZone.run(() => {
            console.log('📩 Yeni mesaj geldi:', chatMessage);

            // 🔸 Sadece bu konuşmaya ait mesajları ekle
            const isForCurrentChat =
              (chatMessage.senderNumber === this.aliciNumara && chatMessage.receiverNumber === this.currentNumber) ||
              (chatMessage.senderNumber === this.currentNumber && chatMessage.receiverNumber === this.aliciNumara);

            if (!isForCurrentChat) return; // başkasının mesajıysa görme

            // 🔹 Yeni mesaj objesi oluştur
            const newMessage = {
              senderNumber: chatMessage.senderNumber,
              receiverNumber: chatMessage.receiverNumber,
              content: chatMessage.content,
              sentAt: new Date(chatMessage.sentAt)
            };

            // 🔹 Aynı mesajı tekrarlamamak için kontrol
            const alreadyExists = this.conversationData.some(
                m =>
                  m.content === newMessage.content &&
                  m.senderNumber === newMessage.senderNumber &&
                  m.receiverNumber === newMessage.receiverNumber &&
                  m.pending === true
              );
            if (alreadyExists) return;

            this.conversationData.push(newMessage);
            this.conversationData.sort((a, b) =>
              new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
            );

            this.scrollToBottom();
          });
        });

        // 🔹 Yazıyor eventi
        this.hubService.hubConnection.off('UserTyping');
        this.hubService.hubConnection.on('UserTyping', (senderNumber: string) => {
          if (senderNumber === this.aliciNumara) {
            this.statusMessage = 'Yazıyor...';
            if (this.typingTimeout) clearTimeout(this.typingTimeout);
            this.typingTimeout = setTimeout(() => (this.statusMessage = ''), 1000);
          }
        });

        // 🔹 Online/Offline durumları
        this.hubService.hubConnection.off('SendHasOnline');
        this.hubService.hubConnection.on('SendHasOnline', (number: string) => {
          if (number === this.aliciNumara) this.statusMessage = 'online';
        });


        this.hubService.hubConnection.off('ReceiveUserLastSeen');
        this.hubService.hubConnection.on('ReceiveUserLastSeen', (phoneNumber: string, lastSeen: string) => {
          if (phoneNumber === this.aliciNumara)
            this.statusMessage = `Son görülme: ${lastSeen}`;
        });

        this.hubService.hubConnection.on('UpdateNotifyClientMessageList', (senderNumber: string, receiverNumber: string) => {
          console.log('📬 Liste güncelle sinyali alındı:', senderNumber, receiverNumber);
         });





        clearInterval(waitForHub);
      }
    }, 300);
  }

  // ✅ Kullanıcı bilgilerini getir
  getMe(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.getmeService.getMe().subscribe({
        next: (res) => {
          this.currentUserId = res.data.id;
          this.currentNumber = res.data.phoneNumber;
          this.currenUserName = res.data.userName;
          resolve();
        },
        error: (err) => {
          console.error('getMe başarısız oldu:', err);
          reject(err);
        }
      });
    });
  }

  onTyping() {
    this.hubService.hubConnection.invoke('UserTyping', this.aliciNumara);
  }

  // ✅ Mesaj gönder
    sendMessage() {
    const content = this.userInputMessage.trim();
    if (!content) return;

    const chatMessageDto: ChatMessage = {
      senderNumber: this.currentNumber,
      receiverNumber: this.aliciNumara,
      content: content,
      sentAt: new Date() // sadece kullanıcı arayüzünde göstermek için
    };

    this.userInputMessage = '';

    // 🔸 Artık burada push ETMİYORUZ.
    // Mesajı doğrudan hub üzerinden alacağız (ReceiveMessageAsync)
    // Bu nedenle sadece API çağrısı yapıyoruz.

    this.sendMessageService.sendMessage(chatMessageDto).subscribe({
      next: () => {
        console.log('✅ Mesaj API üzerinden gönderildi');
      },
      error: (err) => {
        console.error('❌ Mesaj gönderimi başarısız:', err);
      }
    });
  }


  // ✅ Scroll davranışları
  ngAfterViewInit() {
    // Sayfa ilk açıldığında en alta git
    setTimeout(() => this.scrollToBottom(), 300);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['conversationData'] || changes['aliciNumara']) {
      this.resetScroll();
    } else {
      this.scrollToBottom();
    }
  }

  resetScroll() {
    this.ngZone.runOutsideAngular(() => {
      setTimeout(() => {
        const el = this.chatContainer?.nativeElement;
        if (el) {
          el.scrollLeft = 0;
          el.scrollTop = el.scrollHeight;
        }
      }, 100);
    });
  }



  scrollToBottom() {
  this.ngZone.runOutsideAngular(() => {
    setTimeout(() => {
        const el = this.chatContainer?.nativeElement;
        if (el) {
          el.scrollTop = el.scrollHeight; // 🔹 direkt sona git (efekt yok)
        }
      }, 200); // 🔹 hafif gecikme: render tamamlanınca
    });
  }




  trackByMessage(index: number, message: any): string {
  // Eğer backend ID varsa onu kullan
  if (message.id) return message.id.toString();

  // Yoksa kombinasyonla benzersiz key oluştur
  const key = `${message.senderNumber}-${message.receiverNumber}-${message.sentAt}-${message.content}`;
  return key;
}

}
