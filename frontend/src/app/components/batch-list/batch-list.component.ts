import { Component, OnInit } from '@angular/core';
import { Import } from '../../shared/models/Import';
import { BatchProcessorService } from '../../core/services/batch-processor.service';

@Component({
  selector: 'SA-batch-list',
  template: `
    <section>
      <aside *ngIf="!!error" class="error">
        {{ error }}
      </aside>
      <aside *ngIf="!!!processList.length">
        <img src="assets/carpool.png" alt="carpooling image">
        <p>There is no trip yet</p>
        <SA-file-uploader></SA-file-uploader>
      </aside>
      <article>
        <h2 style="text-align: center;">Previous imports</h2>
        <div *ngFor="let process of processList" class="flex process">
          <a class="process-id" [routerLink]=[process.processId]>
            <i class="icofont-dashboard-web"></i>
            {{ process.processId }}
          </a>
          <div class="processed-at">
            <small>{{ process.processedAt }}</small>
          </div>
        </div>
      </article>
    </section>
  `,
  styles: [
    `
      section {
        height: 100%;
      }

      article {
        padding-bottom: 1rem;
      }

      aside {
        align-items: center;
        display: flex;
        flex-direction: column;
        height: 100%;
        justify-content: center;
      }

      aside img {
        filter: grayscale(1);
        max-width: 90%;
      }

      aside p {
        color: gray;
        font-size: 1.5rem;
        font-weight: bold;
        line-height: 0;
      }

      .error {
        color: #ff4136;
      }

      .process {
        border-bottom: 1px solid #0074d9;
        max-width: 800px;
        margin: 0 auto; 
      }

      .process:hover {
        cursor: pointer;
      }

      .process-id:hover {
        font-weight: bold;
      }

      .processed-at {
        display: none;
        text-align: right;
      }

      @media screen and (min-width: 720px) {
        .process-id {
          width: 60%;
        }
        .processed-at {
          display: block;
          width: 40%;
        }
      }
    `
  ]
})
export class BatchListComponent implements OnInit {
  processList: Import[] = [];
  error: string = '';

  constructor(private importService: BatchProcessorService) {
    this.importService = importService;

    importService.getAll()
      .subscribe((result: any) => {
          if (result['status'] === 'success') {
            let data = result['data'];
            this.processList = Object.keys(data).map(k => new Import(k, data[k]));
          }
        },
        err => {
          this.error = "Could not connect to API";
          console.error(err);
        });
  }

  ngOnInit(): void {
  }

}
