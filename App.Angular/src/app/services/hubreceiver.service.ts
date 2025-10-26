import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';
import { ChatMessage } from '../models/test/ChatMessage.model';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class HubreceiverService {
  public hubConnection!: signalR.HubConnection;

  // 📡 Chat listesi güncellendiğinde yayın yapılacak Subject
  private messageListUpdateSource = new Subject<{ senderNumber: string; receiverNumber: string }>();
  messageListUpdate$ = this.messageListUpdateSource.asObservable();

  // 📩 Mesaj geldiğinde yayın yapılacak Subject
  private newMessageSource = new Subject<ChatMessage>();
  newMessage$ = this.newMessageSource.asObservable();

  constructor(
    private zone: NgZone,
    private notificationService: NotificationService
  ) {}

  // ✅ Bağlantı durumu kontrolü için getter
  get isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  async startConnection(): Promise<void> {
    if (this.isConnected) {
      console.log('⚡ SignalR zaten bağlı.');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7281/workerhub', {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    this.registerHubEvents();

    try {
      await this.hubConnection.start();
      console.log('✅ SignalR bağlantısı kuruldu.');
      await this.hubConnection.invoke('LoginSystem');
    } catch (err) {
      console.error('❌ SignalR başlatma hatası:', err);
    }
  }

  // ✅ Liste güncelleme olayını dinleyen observable
  listenMessageListUpdate(): Observable<{ senderNumber: string; receiverNumber: string }> {
    return this.messageListUpdate$;
  }

  private registerHubEvents() {
    // ✅ Yeni mesaj geldiğinde
    this.hubConnection.on('ReceiveMessageAsync', (chatMessage: ChatMessage) => {
      this.zone.run(() => {
        console.log('📩 Yeni mesaj geldi:', chatMessage);
        this.newMessageSource.next(chatMessage);
      });
    });

    // ✅ Liste güncellemesi sinyali geldiğinde
    this.hubConnection.on('UpdateNotifyClientMessageList', (senderNumber: string, receiverNumber: string) => {
      this.zone.run(() => {
        console.log('📬 Liste güncelleme sinyali geldi:', senderNumber, receiverNumber);
        this.messageListUpdateSource.next({ senderNumber, receiverNumber });
      });
    });

    /*
    // 🔹 Ek eventler
    this.hubConnection.on('SendHasOnline', (userNumber: string) => {
      this.zone.run(() => console.log(`🟢 Kullanıcı online: ${userNumber}`));
    });
    */

    this.hubConnection.on('ReceiveUserLastSeen', (userNumber: string, lastSeen: string) => {
      this.zone.run(() => console.log(`⚫ ${userNumber} son görülme: ${lastSeen}`));
    });

    this.hubConnection.on('UserTyping', (senderNumber: string) => {
      this.zone.run(() => console.log(`${senderNumber} yazıyor...`));
    });
  }
}
