import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { GenericService } from './generic.service';


@Injectable({
  providedIn: 'root'
})
export class LoginService {

  private endPoint : string = "auth/login";

  constructor(private _http : HttpClient, private genericService : GenericService){}

  login(userNumber: string, password: string): Observable<any> {
    const body = { userNumber: userNumber, password: password };
    return this.genericService.post<any>(this.endPoint, body).pipe(
      tap(response => {
      })
    );
  }

  //auth işlemleri için kontrol!
  checkAuthentication(): Observable<any>{
    const endPoint = "users/me";
    return this.genericService.get<any>(endPoint).pipe(
      tap(response => {
      })
    );
  }
}
