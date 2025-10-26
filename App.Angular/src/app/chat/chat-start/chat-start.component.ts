import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, NgZone, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Conversation } from '../../models/test/conversation.model';
import { ConversationService } from '../../services/conversation.service';
import { HubreceiverService } from '../../services/hubreceiver.service';
import { SendmessageService } from '../../services/sendmessage.service';



@Component({
  selector: 'app-chat-start',
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-start.component.html',
  styleUrl: './chat-start.component.css'
})
export class ChatStartComponent implements AfterViewInit, OnInit {

  @Input() conversationData: Conversation[] = null; //tamamlandı

  @Input() currentUserId: string; //tamamlandi

  @Input() currentUserNumber: string;

  @Input() senderUserId: string; //

  @Input() senderNumber: string; //tamamlandi

  @Input() benimnumaram: string;

  //En baştan düzeltelim!
  @Input() aliciNumara: string;

  @Input() aliciUserId: string;

  @Input() senderName: string; //DİNLEYELİM!!!!

  userInputMessage: string = '';

  @ViewChild('chatContainer') chatContainer: ElementRef;

  constructor(private hubService : HubreceiverService, private sendMeesageService : SendmessageService, private conversationService : ConversationService, private ngZone: NgZone)
  {
  }

  ngOnInit(): void {
    // SignalR ile mesaj alındığında conversationData'yı güncelle
    this.hubService.listenMessageListUpdate((senderNumber, receiverNumber)=> {
      this.conversationService.getConversations(this.senderNumber).subscribe({
        next: (res) => {
          this.conversationData = res.data;
        },
        error: (err) => {
          console.error('Mesaj listesi güncellenemedi', err);
        }
      });
    })

    this.hubService.listenIncomingMessage((sender, message) => {

      this.conversationService.getConversations(this.senderNumber).subscribe({
        next: (res) => {
          this.conversationData = res.data;
          this.scrollToBottom();
        },
        error: (err) => {
          console.error('Mesaj listesi alınamadı', err);
        }
      });
    });
  }


  sendMessage() {
    const message = this.userInputMessage.trim();
    if (!message) return;

    // 1. SignalR ile gönder
    this.hubService.hubConnection.invoke('SendMessage', this.senderNumber, message);

    // 2. API'ye POST isteği gönder
    this.sendMeesageService.sendMessage(this.senderNumber, message).subscribe({
      next: (res) =>{},
      error: (err) => {
        console.log(this.currentUserId, this.senderNumber  );
      }
    });

    // 3. UI'da göster
    this.conversationData.push({
      id: this.conversationData.length,
      senderId: this.currentUserId,
      senderNumber: this.currentUserNumber,
      receiverId: this.aliciUserId,
      receiverNumber: this.aliciNumara,
      content: message,
      sentAt: new Date()
    });

    this.userInputMessage = '';
    this.scrollToBottom();
  }

  ngAfterViewInit() {
    this.scrollToBottom();
  }

  ngOnChanges() {
    this.scrollToBottom(); // Yeni mesaj gelince en alta git
  }

  scrollToBottom() {
    this.ngZone.runOutsideAngular(() => {
      setTimeout(() => {
        const el = this.chatContainer?.nativeElement;
        if (el) {
          el.scrollTop = el.scrollHeight;
        }
      }, 100); // DOM render tamamlandıktan sonra kaydır
    });
  }

}
