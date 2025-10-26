import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GenericService {

  //https://localhost:7281/api/auth/login     https://fahrigedik.shop/api

  private baseUrl : string = "https://localhost:7281/api";

  constructor(private http : HttpClient) { }

  get<T>(endpoint: string, params?: any) {
    const httpParams = this.createHttpParams(params);

    return this.http.get<{data : T}>(`${this.baseUrl}/${endpoint}`, {
      params: httpParams,
      withCredentials: true
    });
  }

  post<T>(endpoint: string, body: any) {
    return this.http.post<{data : T}>(`${this.baseUrl}/${endpoint}`, body, {
      withCredentials: true
    });
  }

  private createHttpParams(params: any): HttpParams {
    let httpParams = new HttpParams();
    if (params) {
      for (const key of Object.keys(params)) {
        httpParams = httpParams.set(key, params[key]);
      }
    }
    return httpParams;
  }
}
