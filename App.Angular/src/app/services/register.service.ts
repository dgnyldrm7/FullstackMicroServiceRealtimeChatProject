import { Injectable } from '@angular/core';
import { GenericService } from './generic.service';

@Injectable({
  providedIn: 'root'
})
export class RegisterService {

  private endpoint: string = 'users';

  constructor(private genericService: GenericService) { }

  public register(userName: string, email: string, password: string, phoneNumber: string) {
    const body = {
      userName: userName,
      email: email,
      password: password,
      phoneNumber: phoneNumber,
    };
    return this.genericService.post(this.endpoint, body);
  }
}
