import { Component } from '@angular/core';
import { HubreceiverService } from '../../services/hubreceiver.service';

@Component({
  selector: 'app-chat-box',
  imports: [],
  templateUrl: './chat-box.component.html',
  styleUrl: './chat-box.component.css'
})
export class ChatBoxComponent {
  messages: any[] = [];

  constructor(private messageService: HubreceiverService) {}

  ngOnInit(): void {
    this.messageService.startConnection();

  }
}
