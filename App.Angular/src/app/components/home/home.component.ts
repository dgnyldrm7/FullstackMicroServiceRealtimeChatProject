import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ChatBoxComponent } from '../../chat/chat-box/chat-box.component';
import { ChatListBoxComponent } from '../../chat/chat-list-box/chat-list-box.component';
import { ChatNewComponent } from '../../chat/chat-new/chat-new.component';
import { ChatStartComponent } from '../../chat/chat-start/chat-start.component';
import { Conversation } from '../../models/test/conversation.model';
import { MessageContext } from '../../models/test/messagecontext.model';
import { ConversationService } from '../../services/conversation.service';
import { GetuserService } from '../../services/getuser.service';

@Component({
  selector: 'app-home',
  imports: [ChatBoxComponent, ChatListBoxComponent, ChatNewComponent, ChatStartComponent, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {

  selectedConversationData: Conversation[] = null; //tamamlandi.

  messageContext: MessageContext = {
    senderId: null,
    receiverId: null,
    friendNumber: null,
  };

  // Yeni değişkenler
  currentUserId: string = 'sabittir!'; // Örnek olarak bir kullanıcı ID'si
  friendNumber: string;
  friendName: string;

  isSelected: boolean = false; //tamamlandi.

  constructor(private conversationService : ConversationService, private getUserService : GetuserService)
  {
  }

  handleFriendName(friendName : string)
  {
    this.friendName = friendName;
  }

  handleFriendNumber(friendNumber: string) {

    this.conversationService.getConversations(friendNumber).subscribe({
      next: (response) => {

        this.selectedConversationData = response.data;

        this.messageContext.friendNumber = friendNumber;

        // Tüm konuşmaları işleyerek senderId ve receiverId bilgilerini alıyoruz
        if (response.data.length > 0) {

          // İlk konuşmayı varsayılan olarak seçiyoruz
          this.messageContext.senderId = response.data[0].senderId;
          this.messageContext.receiverId = response.data[0].receiverId;
        }

        this.isSelected = true;
      },
      error: (error) => {
        console.error('Konuşma verileri alınırken hata oluştu:', error);
      }
    });
  }


  selectConversation(conversation: Conversation) {
    this.messageContext.senderId = conversation.senderId;
    this.messageContext.receiverId = conversation.receiverId;
    console.log('Seçilen konuşma bilgileri güncellendi:', this.messageContext);
  }


  handleSenderUserId(senderUserId: string) {
    const matchedConversation = this.selectedConversationData?.find(
      (conversation) => conversation.senderId === senderUserId
    );

    if (matchedConversation) {
      this.messageContext.senderId = matchedConversation.senderId;
      console.log('Sender user ID eşleşti ve atandı:', this.messageContext.senderId);
    } else {
      console.warn('Sender user ID ile eşleşen bir konuşma bulunamadı.');
    }
  }
}
