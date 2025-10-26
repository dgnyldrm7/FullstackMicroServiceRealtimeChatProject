import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { Subscription } from 'rxjs';
import { Friend } from '../../models/test/friend.model';
import { FriendsService } from '../../services/friends.service';
import { GetuserService } from '../../services/getuser.service';
import { HubreceiverService } from '../../services/hubreceiver.service';

@Component({
  selector: 'app-chat-list-box',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chat-list-box.component.html',
  styleUrl: './chat-list-box.component.css',
})
export class ChatListBoxComponent implements OnInit, OnDestroy {

  friends: Friend[] = [];
  private subscription = new Subscription();

  constructor(
    private friendsService: FriendsService,
    private hubService: HubreceiverService,
    private getUserService: GetuserService
  ) {}

  //Child component'tan parent component'a veri göndermek için EventEmitter kullanıyoruz
  @Output() friendNumber = new EventEmitter<string>();

  @Output() senderSelected = new EventEmitter<string>();

  @Input() friendName: string;

  @Output() _friendName = new EventEmitter<string>();

  @Output() friendNameSelected = new EventEmitter<string>();

  _friendNameSelected: string;

  isLoading: boolean = true;
  initialLoadCompleted: boolean = false;



  sendDataToParent(friendNumber: string)
  {
    this.friendNumber.emit(friendNumber);
    this.getUserInfo(friendNumber);
  }

  getUserInfo(frindNumber : string)
  {
    const sub = this.getUserService.getUserByNumber(frindNumber).subscribe({
      next: (user) => {
        this._friendNameSelected = user.data.userName;
        this.friendNameSelected.emit(this._friendNameSelected);
      },
      error: (error) => {
        console.error('Arkadaşlar alınırken hata oluştu:', error);
      }
    });
    this.subscription.add(sub);
  }

  ngOnInit(): void {
    this.loadFriends();

    // Hub bağlantısının hazır olmasını bekleyip sonra event'i dinle
    const waitForConnection = setInterval(() => {
      if (this.hubService.startConnection) {
        this.hubService.listenMessageListUpdate((receiverNumber: string) => {
          this.loadFriends();
        });
        clearInterval(waitForConnection);
      }
    }, 500);
  }


  loadFriends() {
    // Sadece ilk kez yüklenirken loading göster
    if (!this.initialLoadCompleted) {
      this.isLoading = true;
    }

    const sub = this.friendsService.getFriends().subscribe({
      next: (friends) => {
        this.friends = friends.data.sort((a, b) => {
          return new Date(b.lastMessageSentAt).getTime() - new Date(a.lastMessageSentAt).getTime();
        });

        // İlk yükleme tamamlandıysa flag'i ayarla
        if (!this.initialLoadCompleted) {
          this.initialLoadCompleted = true;
          this.isLoading = false;
        }
      },
      error: (error) => {
        console.error('Arkadaşlar alınırken hata oluştu:', error);
        if (!this.initialLoadCompleted) {
          this.isLoading = false;
        }
      }
    });

    this.subscription.add(sub);
  }



  ngOnDestroy(): void {
    this.subscription.unsubscribe(); // bellek sızıntısı önlenir
  }
}
