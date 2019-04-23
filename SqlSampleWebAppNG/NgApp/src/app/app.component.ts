
import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BackendApiService } from '../app/backend-api.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent  implements OnInit {

  selectedColor:string = "RED";
  colors = Array<string>();
  products = Array<any>();
  title = 'SqlSampleWebAppNG';
  
  constructor(private http: HttpClient, private backendApiService: BackendApiService) { /* foo */  }

  ngOnInit(): void {

    this.colors.push('RED');    
    this.colors.push('WHITE');    
    this.colors.push('BLUE');

    this.backendApiService.getApiData(null).subscribe(apiRows => this.products = apiRows);

  }

  onButtonClick(){
    this.backendApiService.getApiData(this.selectedColor).subscribe(apiRows => this.products = apiRows);
  }
}
