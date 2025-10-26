import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private isInChatView = new BehaviorSubject<boolean>(false);
  private hasNotificationPermission = false;
  private notificationSound: HTMLAudioElement;

  constructor() {
    this.requestNotificationPermission();
    // Bildirim sesi için bir Audio nesnesi oluştur
    this.notificationSound = new Audio('assets/notification.mp3');
  }

  private async requestNotificationPermission() {
    if ('Notification' in window) {
      const permission = await Notification.requestPermission();
      this.hasNotificationPermission = permission === 'granted';
    }
  }

  setInChatView(inChat: boolean) {
    this.isInChatView.next(inChat);
  }

  isUserInChat(): boolean {
    return this.isInChatView.getValue();
  }

  showNotification(senderNumber: string, message: string) {
    // Eğer kullanıcı chat ekranında değilse ve bildirim izni varsa
    if (!this.isUserInChat() && this.hasNotificationPermission) {
      // Bildirim göster
      const notification = new Notification(senderNumber + ' tarafından yeni mesaj', {
        body: message,
        icon: '/assets/chat-icon.png',
        badge: '/assets/notification-badge.png'
      });

      // Bildirim sesini çal
      this.notificationSound.currentTime = 0; // Sesi başa sar
      this.notificationSound.play().catch(error => console.log('Ses çalma hatası:', error));

      // Bildirime tıklandığında
      notification.onclick = () => {
        window.focus(); // Pencereyi öne getir
        notification.close();
      };
    }
  }
}
