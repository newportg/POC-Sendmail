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
    this.hubConnection.on("newHubEvent", (event) => {
      this.events.push(event);
      var jsonObject: any = JSON.parse(event);

      for (let y = 0; y < jsonObject.records.length; y++) {
        var result = this.eventss.find(o => o.id === jsonObject.records[y].correlationId);
        if (result == null) {
          var it = this.eventss.push(new Item(jsonObject.records[0].correlationId, false, false, false, ""));
          this.eventss[it-1].UpdateHub(jsonObject.records[y]);
        }
        if (result != null) {
          result.UpdateHub(jsonObject.records[y]);
        }
      }
    });

    this.hubConnection.on("newGridEvent", (event) => {
      this.events.push(event);
      var jsonObject: any = JSON.parse(event);

      var result = this.eventss.find(o => o.id === jsonObject.data.messageId);
      if (result == null) {
        var it = this.eventss.push(new Item(jsonObject.data.messageId, false, false, false, ""));
        this.eventss[it - 1].UpdateGrid(jsonObject);
      }
      if (result != null) {
        result.UpdateGrid(jsonObject);
      }
    });
  }
}

export class Item {
  public id: string;
  public hsent: boolean;
  public hdelivered: boolean;
  public hviewed: boolean;
  public hstatus: string;
  public gsent: boolean;
  public gdelivered: boolean;
  public gviewed: boolean;
  public gstatus: string;

  constructor(id: string, sent: boolean, delivered: boolean, viewed: boolean, status: string) {
    this.id = id;
    this.hsent = sent;
    this.hdelivered = delivered;
    this.hviewed = viewed;
    this.hstatus = status;

    this.gsent = sent;
    this.gdelivered = delivered;
    this.gviewed = viewed;
    this.gstatus = status;
  }

  public UpdateHub(source: any) {

    if (source.properties.OperationCategory == "EmailSendMailOperational") {
      if (source.properties.OperationType == "SendMail") {
        this.hstatus = "Queued";
      }
    }
    else if (source.properties.OperationCategory == "EmailStatusUpdateOperational") {
      if (source.properties.OperationType == "GetMessageStatus") {
        this.hstatus = source.properties.MessageStatus;
        if (source.properties.MessageStatus == "Succeeded") {
          this.hsent = true;
        }
      }
      if (source.properties.OperationType == "DeliveryStatusUpdate") {
        this.hstatus = source.properties.DeliveryStatus;
        if (source.properties.DeliveryStatus == "Delivered") {
          this.hdelivered = true;
        }
      }
    }
    else if (source.properties.OperationCategory == "EmailUserEngagementOperational") {
      if (source.properties.OperationType == "UserEngagementUpdate") {
        if (source.properties.EngagementType == "View") {
          this.hviewed = true;
        }
        else if (source.properties.EngagementType == "Click") {
          this.hstatus = "Clicked :" + source.data.engagementContext;
          this.hviewed = true;
        }
      }
    }
    else {
      this.hstatus = "Unknown :" + source.operationName;
    }
  }

  public UpdateGrid(source: any) {

    if (source.eventType == "Microsoft.Communication.EmailDeliveryReportReceived") {
      if (source.data.status == "Delivered") {
        this.gstatus = "Delivered";
        this.gdelivered = true;
      }
      if (source.data.status == "Expanded") {
        this.gstatus = "Expanded";
      }
      if (source.data.status == "Failed") {
        this.gstatus = "Failed";
      }
    }
    else if (source.eventType == "Microsoft.Communication.EmailEngagementTrackingReportReceived") {
      if (source.data.engagementType == "view") {
        this.gstatus = "Viewed";
        this.gviewed = true;
      }
      else if (source.data.engagementType == "click") {
        this.gstatus = "Clicked :" + source.data.engagementContext;
        this.gviewed = true;
      }
    }
    else {
      this.gstatus = "Unknown :" + source.eventType;
    }
  }
}


