import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HubreceiverService } from '../../services/hubreceiver.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-chat',
  imports: [FormsModule, CommonModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit, OnDestroy {

  messages: string[] = [];

  constructor(
    private service: HubreceiverService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    // Kullanıcı chat görünümünde olduğunu bildir
    this.notificationService.setInChatView(true);
  }

  ngOnDestroy(): void {
    // Kullanıcı chat görünümünden çıktığını bildir
    this.notificationService.setInChatView(false);
  }
}
