import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TestService {

  constructor(private http : HttpClient) { }

  GetTodosDataWithModel()
  {
    return this.http.get<any[]>('https://jsonplaceholder.typicode.com/todos');
  }
}
