import { HttpEventType } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BatchProcessorService } from '../../core/services/batch-processor.service';

@Component({
  selector: 'SA-file-uploader',
  template: `
    <input type="file" id="input-file-upload" (change)="OnFileChange($event)"/>
    <button id="btn-upload" (click)="triggerFileInput($event)">
      <i class="icofont-cloud-upload"></i>
      Upload file
    </button>
  `,
  styles: [`
    input[type="file"] {
      display: none;
    }    
  `]
})
export class FileUploaderComponent implements OnInit {

  @Input() toggleSpinner: Function;
  uploadService: BatchProcessorService;

  constructor(uploadService: BatchProcessorService, private router: Router) {
    this.uploadService = uploadService;
  }

  ngOnInit(): void {
  }

  OnFileChange(event: any) {
    this.toggleSpinner(true);
    document.getElementById('btn-upload')?.setAttribute('disabled', 'disabled');
    
    let files = event.target.files;
    if (!!!files.length) {
      this.toggleSpinner(false);
      document.getElementById('btn-upload')?.removeAttribute('disabled');
      return;
    }

    this.uploadService.uploadFile(files[0])
      .subscribe(
        event => {
          if (event.type == HttpEventType.Response) {
            this.toggleSpinner(false);
            document.getElementById('btn-upload')?.removeAttribute('disabled');
            let body: any = event.body;
            this.router.navigate([body['data'] ?? '']);
          }
        },
        err => {
          alert('Could not connect to API');
          console.error(err);
          this.toggleSpinner(false);
          document.getElementById('btn-upload')?.removeAttribute('disabled');
          },
        () => console.info('Request completed'));
  }

  triggerFileInput(event: any) {
    let element: HTMLInputElement = document.getElementById('input-file-upload') as HTMLInputElement;
    element.value = '';
    element.click();
    event.preventDefault();
  }

}
