import { Component } from '@angular/core';
import * as SignalR from '@microsoft/signalr';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  title = 'viewer-app';
  events: string[] = [];
  eventss: Item[] = [];
  baseurl: string;
/*  countryData: any;*/

  private hubConnection: SignalR.HubConnection;

  public open() {
    //alert("button was clicked");
    return this.http.get('https://func-poc-sendmail-vse-ne.azurewebsites.net/api/SendMail')
  }

  constructor(private http: HttpClient) {
    // Create connection
    this.hubConnection = new SignalR.HubConnectionBuilder()
      .withUrl("https://func-poc-sendmail-vse-ne.azurewebsites.net/api/")
      .configureLogging(SignalR.LogLevel.Debug)
      .build();

    // Start connection. This will call negotiate endpoint
    console.log('connecting...');

    this.hubConnection
      .start()
      .then((response) => console.log("Connection Started", response))
      .catch(err => console.log('Error while starting connection: ' + err))

    this.baseurl = this.hubConnection.baseUrl;

    // Handle incoming events for the specific target
    this.hubConnection.on("newEvent", (event) => {
      this.events.push(event);

      this.eventss.push(new Item(event.messageId, event.status == "Delivered"?true:false, event.engagementType == "view" ? true:false));
    });

    this.eventss.push( new Item( "1234456", true, false) );

  }
}

export class Item {
  public id: string;
  public sent: boolean;
  public viewed: boolean;

  constructor(id: string, sent: boolean, viewed: boolean) {
    this.id = id;
    this.sent = sent;
    this.viewed = viewed;
  }
}
