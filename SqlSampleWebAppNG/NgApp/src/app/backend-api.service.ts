
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BackendApiService {

  constructor(private http: HttpClient) { }

  getApiData(color): Observable<any[]> {

    let qs:string = "?skip=0&take=10";
    if (color) {
      qs = qs + "&color=" + color;
    }

    return this.http.get<any[]>("api/products" + qs);

  }
}
