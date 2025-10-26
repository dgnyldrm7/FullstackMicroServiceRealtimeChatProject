import { Injectable } from '@angular/core';
import { GenericService } from './generic.service';

@Injectable({
  providedIn: 'root'
})
export class LogoutService {

  private endPoint: string = 'auth/web/logout';

  constructor(private genericService :GenericService) { }

  logout() {
    return this.genericService.post(this.endPoint, {});
  }
}
