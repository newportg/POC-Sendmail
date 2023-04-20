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
  emailid: any;

  private hubConnection: SignalR.HubConnection;

  public open() {
    //alert("button was clicked");

    this.http.get('https://func-poc-sendmail-vse-ne.azurewebsites.net/api/SendMail', { observe: 'response', responseType: 'text' })
      .subscribe(
        (response) => {
          this.emailid = response.body?.toString();
          this.eventss.push(new Item(this.emailid, false, false));
        },
      (error) => { console.log(error); });
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
      var jsonObject: any = JSON.parse(event);

      for (let i = 0; i < this.eventss.length; i++) {
        if (this.eventss[i].id == jsonObject.messageId) {
          if (jsonObject.status == "Delivered")
            this.eventss[i].delivered = true;
          if (jsonObject.engagementType == "view")
            this.eventss[i].viewed = true;
        }
      }

      var result = this.eventss.find(o => o.id === jsonObject.messageId);
      if( result == null)
        this.eventss.push(new Item(jsonObject.messageId, jsonObject.status == "Delivered" ? true : false, jsonObject.engagementType == "view" ? true:false));
    });

  }
}

export class Item {
  public id: string;
  public delivered: boolean;
  public viewed: boolean;

  constructor(id: string, delivered: boolean, viewed: boolean) {
    this.id = id;
    this.delivered = delivered;
    this.viewed = viewed;
  }
}
