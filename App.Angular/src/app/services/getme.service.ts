import { Injectable } from '@angular/core';
import { GenericService } from './generic.service';
import { Me } from '../models/test/me.model';

@Injectable({
  providedIn: 'root'
})
export class GetmeService {

  readonly endPoint : string = "users/me";

  constructor(private genericService : GenericService) { }

  getMe()
  {
    return this.genericService.get<Me>(this.endPoint);
  }
}
