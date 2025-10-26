import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
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
export class ChatNewComponent {

  errorMessage: string = ""; // Hata mesajı için değişken

  @ViewChild('closeModalButton') closeModalButton: ElementRef;

  showUser : User = {
    id: "",
    userName: "",
    email: "",
    phoneNumber: ""
  }

  userNumber : string; // Kullanici numarasi tutmak icin!

  isLoading: boolean = false;  // Loading durumunu kontrol etmek için

  targetNumber: string = ""; // Hedef numara
  messageContent: string = ""; // Mesaj içeriği

  constructor(private getMe: GetmeService, private logoutService: LogoutService, private router: Router, private messageService : SendmessageService) { }

  newMessage() {
    this.errorMessage = "";
    if (!this.messageContent || this.messageContent.trim() === "") {
      this.errorMessage = "Mesaj içeriği boş olamaz.";
      return;
    }
    this.messageService.sendMessage(this.targetNumber, this.messageContent).subscribe({
      next: (response) => {

        this.closeModalButton.nativeElement.click(); // ✅ Modal'ı kapat

        // Alanları temizle
        this.targetNumber = "";
        this.messageContent = "";
      },
      error: (error) => {
        console.error("Mesaj gönderimi başarısız oldu: ", error);
        this.errorMessage = "Mesaj gönderilirken bir hata oluştu. Lütfen tekrar deneyin.";
      },
      complete: () => {
      }
    });
  }



  getUserInfo() {
    this.getMe.getMe().subscribe((response) => {
      this.userNumber = response.data.phoneNumber;
      this.showUser = response.data;
    });
  }

  logOut() {
    this.isLoading = true;
    this.getUserInfo();
    // 1 saniye bekle, ardından logout işlemini başlat
    setTimeout(() => {
      this.logoutService.logout().subscribe({
        next: (response) => {
          this.router.navigate(['/login']);
        },
        error: (error) => {
          console.error("Logout failed, çözümlenilemeyen hata! ", error);
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    }, 1000);
  }

}
