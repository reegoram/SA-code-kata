import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})  
export class BatchProcessorService {
  apiUrl: string;

  constructor(private httpClient: HttpClient) {
    this.apiUrl = environment.apiUrl + 'batchImports';
  }

  getInfo(processId: string) {
    return this.httpClient.get(this.apiUrl + '/' + processId);
  }

  getAll() {
    return this.httpClient.get(this.apiUrl);
  }

  uploadFile(file: File) {
    let formData = new FormData();
    formData.append("file[]", file, file.name);
    
    return this.httpClient.post(this.apiUrl, formData, {
      reportProgress: true,
      observe: 'events'
    });
  }
}