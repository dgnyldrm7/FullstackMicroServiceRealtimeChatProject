import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class HubreceiverService {
  public hubConnection!: signalR.HubConnection;

  constructor() {}

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://fahrigedik.shop/workerhub', {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(()=>{
        console.log('SignalR bağlantısı başlatıldı.');
      })
      .catch(err => {
        console.error('Error while starting SignalR connection: ', err);
      });
  }

  listenMessageListUpdate(callback: (senderNumber: string, receiverNumber: string) => void) {
    if (!this.hubConnection) {
      console.warn('Hub bağlantısı hazır değil.');
      return;
    }

    this.hubConnection.on('UpdateNotifyClientMessageList', callback);
  }

  listenIncomingMessage(callback: (receiverNumber: string, message: string) => void) {
    if (!this.hubConnection) {
      console.warn('Hub bağlantısı hazır değil.');
      return;
    }

    this.hubConnection.on('SendMessage', (receiverNumber: string, message: string) => {
      callback(receiverNumber, message);
    });
  }
}
