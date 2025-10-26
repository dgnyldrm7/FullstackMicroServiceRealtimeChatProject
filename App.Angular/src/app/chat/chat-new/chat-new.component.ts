import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core'; // 🔹 OnInit eklendi
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ChatMessage } from '../../models/test/ChatMessage.model';
import { User } from '../../models/test/user.model';
import { GetmeService } from '../../services/getme.service';
import { LogoutService } from '../../services/logout.service';
import { SendmessageService } from '../../services/sendmessage.service';

@Component({
  selector: 'app-chat-new',
  templateUrl: './chat-new.component.html',
  styleUrls: ['./chat-new.component.css'],
  imports: [CommonModule, FormsModule]
})
export class ChatNewComponent implements OnInit {   // 🔹 implements OnInit eklendi

  errorMessage: string = "";
  @ViewChild('closeModalButton') closeModalButton: ElementRef;

  showUser : User = {
    id: "",
    userName: "",
    email: "",
    phoneNumber: ""
  };

  userNumber : string;
  isLoading: boolean = false;
  targetNumber: string = "";
  messageContent: string = "";

  constructor(
    private getMe: GetmeService,
    private logoutService: LogoutService,
    private router: Router,
    private messageService : SendmessageService
  ) { }

  ngOnInit(): void {                      // ✅ Kullanıcı bilgisi component açıldığında alınır
    this.getUserInfo();
  }

  newMessage() {
    this.errorMessage = "";

    if (!this.userNumber) {
      this.errorMessage = "Kullanıcı numarası yüklenemedi. Lütfen tekrar deneyin.";
      return;
    }

    if (!this.messageContent || this.messageContent.trim() === "") {
      this.errorMessage = "Mesaj içeriği boş olamaz.";
      return;
    }

    const chatMessageDto: ChatMessage = {
      senderNumber: this.userNumber,
      receiverNumber: this.targetNumber,
      content: this.messageContent,
      sentAt: new Date()
    };

    this.messageService.sendMessage(chatMessageDto).subscribe({
      next: () => {
        this.closeModalButton.nativeElement.click();
        this.targetNumber = "";
        this.messageContent = "";
      },
      error: (error) => {
        console.error("Mesaj gönderimi başarısız oldu: ", error);
        this.errorMessage = "Mesaj gönderilirken bir hata oluştu. Lütfen tekrar deneyin.";
      }
    });
  }

  getUserInfo() {
    this.getMe.getMe().subscribe({
      next: (response) => {
        this.userNumber = response.data.phoneNumber;   // ✅ Artık burada set edilecek
        this.showUser = response.data;
      },
      error: (err) => {
        console.error("Kullanıcı bilgisi alınamadı:", err);
        this.errorMessage = "Kullanıcı bilgisi alınamadı. Lütfen giriş yapın.";
      }
    });
  }

  logOut() {
    this.isLoading = true;
    setTimeout(() => {
      this.logoutService.logout().subscribe({
        next: () => this.router.navigate(['/login']),
        error: (error) => {
          console.error("Logout failed: ", error);
          this.isLoading = false;
        },
        complete: () => this.isLoading = false
      });
    }, 1000);
  }
}
