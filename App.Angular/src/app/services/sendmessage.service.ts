import { Injectable } from '@angular/core';
import { ChatMessage } from '../models/test/ChatMessage.model';
import { GenericService } from './generic.service';

@Injectable({
  providedIn: 'root'
})
export class SendmessageService {

  readonly endPoint : string = "chat/messages";

  constructor(private genericService : GenericService) { }

  sendMessage(chatMessageDto: ChatMessage) {
    const body = {
      receiverNumber: chatMessageDto.receiverNumber,
      content: chatMessageDto.content,
      sentAt: chatMessageDto.sentAt,
      senderNumber: chatMessageDto.senderNumber
    };

    return this.genericService.post(this.endPoint, body);
  }


}
