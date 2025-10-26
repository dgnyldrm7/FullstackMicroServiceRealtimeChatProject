import { Injectable } from '@angular/core';
import { GenericService } from './generic.service';

@Injectable({
  providedIn: 'root'
})
export class SendmessageService {

  readonly endPoint : string = "chat/messages";

  constructor(private genericService : GenericService) { }

  sendMessage(receiverUserNumber: string, content: string) {
    const body = {
      receiverUserNumber: receiverUserNumber,
      content: content
    };

    return this.genericService.post(this.endPoint, body);
  }
}
