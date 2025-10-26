import { EventEmitter, Injectable } from '@angular/core';
import { User } from '../models/test/user.model';
import { GenericService } from './generic.service';

@Injectable({
  providedIn: 'root'
})
export class GetuserService {

  private endPoint: string = "users/by-number";

  constructor(private genericService: GenericService) { }

  getUserByNumber(userNumber: string) {
    return this.genericService.get<User>(`${this.endPoint}/${userNumber}`);
  }
}
