import { Component } from '@angular/core';
// import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  title = 'app';
  constructor(
    // private http: HttpClient
  ) {
    // this.http.post('http://localhost:5007/api/instruments/get', {}).subscribe(x => {
    //   console.log(x);
    // });
  }
}
