import { Injectable } from '@angular/core';
import { Friend } from '../models/test/friend.model';
import { GenericService } from './generic.service';

@Injectable({
  providedIn: 'root'
})
export class FriendsService {

  readonly endPoint : string = "chat/friends";

  constructor(private genericService : GenericService) { }

  getFriends()
  {
    return this.genericService.get<Friend[]>(this.endPoint);
  }
}
