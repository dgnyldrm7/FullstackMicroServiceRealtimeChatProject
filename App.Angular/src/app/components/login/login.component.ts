import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LoginService } from '../../services/loginservice.service';
import { RegisterService } from '../../services/register.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  public validationErrors: { [key: string]: string[] } = {};
  public phoneNumber: string = '';
  public password: string = '';
  public userName: string = '';
  public email: string = '';
  public isLoading: boolean = false;
  public isRegistering: boolean = false;
  public successMessage: string = '';
  public errorMessage: string = '';
  public isSuccess: boolean = false;
  public isRegisterSuccess: boolean = false;


  constructor(private loginService : LoginService, private registerService: RegisterService,private router : Router) {}

  public LoginMethod() {
    this.isLoading = true;

    this.loginService.login(this.phoneNumber, this.password)
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          this.isSuccess = false;
          this.router.navigate(['/']);
        },
        error: (error) => {
          this.isLoading = false;
          this.isSuccess = true;
          console.error('Login failed!', error);
        }
      });
  }


  public RegisterMethod() {
    this.isLoading = true;
    this.successMessage = '';
    this.errorMessage = '';
    this.validationErrors = {};
    this.isRegisterSuccess = false;

    this.registerService.register(this.userName, this.email, this.password, this.phoneNumber)
      .subscribe({
        next: (response) => {
          this.successMessage = 'Kayıt başarılı! Giriş sayfasına yönlendiriliyorsunuz.';
          this.isLoading = false;

          // Giriş formuna geçmeden önce 2 saniye mesajı göster
          setTimeout(() => {
            this.isRegistering = false;
            this.router.navigate(['/login']);
            this.phoneNumber = '';
            this.userName = '';
            this.password = '';
          }, 3000);
        },
        error: (error) => {
          this.isLoading = false;

          const errorData = error.error;

          if (errorData?.errors) {
            for (const err of errorData.errors) {
              this.validationErrors[err.field] = err.errors;
            }
            this.errorMessage = 'Lütfen tüm alanları doğru bir şekilde doldurduğunuzdan emin olun.';
          } else {
            this.errorMessage = error.message?.errorMessage || 'Kayıt işlemi sırasında bir hata oluştu.';
          }
        }
      });
  }


  toggleForm(event: Event): void {
    event.preventDefault(); // Sayfa yönlenmesini engelle
    this.isRegistering = !this.isRegistering;
  }
}
