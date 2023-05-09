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
          this.eventss.push(new Item(this.emailid, false, false, false, ""));
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

    //Handle incoming events for the specific target
    this.hubConnection.on("newEvent", (event) => {
      this.events.push(event);
      var jsonObject: any = JSON.parse(event);

    //for (let y = 0; y < jsonObject.records.length; y++) {
    //  for (let i = 0; i < this.eventss.length; i++) {
    //    if (this.eventss[i].id == jsonObject.records[y].correlationId) {
    //      if (jsonObject.records[y].operationName == "SendMail") {
    //        this.eventss[i].sent = true;
    //      }
    //      if (jsonObject.records[y].operationName == "GetMessageStatus") {
    //        this.eventss[i].status = jsonObject.records[y].properties.MessageStatus;
    //        if (jsonObject.records[y].properties.MessageStatus == "Succeeded") {
    //          this.eventss[i].delivered = true;
    //        }
    //      }
    //      if (jsonObject.records[y].operationName == "UserEngagementUpdate") {
    //        if (jsonObject.records[y].properties.engagementType == "View")
    //          this.eventss[i].viewed = true;
    //      }
    //      if (jsonObject.records[y].operationName == "DeliveryStatusUpdate") {
    //        this.eventss[i].status = jsonObject.records[y].properties.DeliveryStatus;
    //      }
    //    }
    //  }
    //}

      var result = this.eventss.find(o => o.id === jsonObject.records[0].correlationId);
      if (result == null) {
        this.eventss.push(new Item(jsonObject.records[0].correlationId, jsonObject.status == "Sent" ? true : false, jsonObject.status == "Delivered" ? true : false, jsonObject.engagementType == "view" ? true : false, ""));
      }

      for (let y = 0; y < jsonObject.records.length; y++) {
        var result = this.eventss.find(o => o.id === jsonObject.records[y].correlationId);
        if (result == null) {
          var it = this.eventss.push(new Item(jsonObject.records[0].correlationId, false, false, false, ""));
          this.eventss[it].Update(jsonObject[y]);
        }
        if (result != null) {
          result.Update(jsonObject[y]);
        }
      }

    });

    //}

    //// Handle incoming events for the specific target
    //this.hubConnection.on("newEvent", (event) => {
    //  this.events.push(event);
    //  var jsonObject: any = JSON.parse(event);

    //  for (let i = 0; i < this.eventss.length; i++) {
    //    if (this.eventss[i].id == jsonObject.messageId) {
    //      if (jsonObject.status == "Delivered")
    //        this.eventss[i].delivered = true;
    //      if (jsonObject.engagementType == "view")
    //        this.eventss[i].viewed = true;
    //    }
    //  }

    //  var result = this.eventss.find(o => o.id === jsonObject.messageId);
    //  if( result == null)
    //    this.eventss.push(new Item(jsonObject.messageId, jsonObject.status == "Delivered" ? true : false, jsonObject.engagementType == "view" ? true:false));
    //});

  }
}

//export class Item {
//  public id: string;
//  public delivered: boolean;
//  public viewed: boolean;

//  constructor(id: string, delivered: boolean, viewed: boolean) {
//    this.id = id;
//    this.delivered = delivered;
//    this.viewed = viewed;
//  }
//}

export class Item {
  public id: string;
  public sent: boolean;
  public delivered: boolean;
  public viewed: boolean;
  public status: string;

  constructor(id: string, sent: boolean, delivered: boolean, viewed: boolean, status: string) {
    this.id = id;
    this.sent = sent;
    this.delivered = delivered;
    this.viewed = viewed;
    this.status = status;
  }

  public Update(source: any) {
    if (source.properties.OperationCategory == "EmailSendMailOperational") {
      if (source.properties.OperationType == "SendMail") {
        this.status = "Queued";
      }
    }
    else if (source.properties.OperationCategory == "EmailStatusUpdateOperational") {
      if (source.properties.OperationType == "GetMessageStatus") {
        this.status = source.properties.MessageStatus;
        if (source.properties.MessageStatus == "Succeeded") {
          this.sent = true;
        }
      }
      if (source.properties.OperationType == "DeliveryStatusUpdate") {
        this.status = source.properties.DeliveryStatus;
        if (source.properties.DeliveryStatus == "Delivered") {
          this.delivered = true;
        }
      }
    }
    else if (source.properties.OperationCategory == "EmailUserEngagementOperational") {
      if (source.properties.OperationType == "UserEngagementUpdate") {
        if (source.properties.EngagementType == "View") {
          this.viewed = true;
        }
      }
    }
    else {
      this.status = "Unknown :" + source.operationName;
    }
  }

}


