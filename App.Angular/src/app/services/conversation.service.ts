import { Injectable } from '@angular/core';
import { Conversation } from '../models/test/conversation.model';
import { GenericService } from './generic.service';

@Injectable({
  providedIn: 'root'
})
export class ConversationService {

  readonly endPoint : string = "chat/conversations";

  constructor(private genericService : GenericService) { }

  getConversations(receiverNumber: string)
  {
    return this.genericService.get<Conversation[]>(`${this.endPoint}/${receiverNumber}`);
  }
}
