import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BatchProcessorService } from '../../core/services/batch-processor.service';

@Component({
  selector: 'SA-trip-summary',
  template: `
    <section>
      <aside class="error" *ngIf="error">
        {{ error }}
      </aside>
      <aside class="info" *ngIf="info">
        {{ info }}
      </aside>
      <article *ngIf="data?.length" class="flex two three-600">
        <div *ngFor="let driverStat of data" class="flex">
          <div>
            <i class="icofont-user-alt-2"></i>
            <span>{{ driverStat.driver.name }}</span>
          </div>
          <div>
            <i class="icofont-car-alt-1"></i>
            <span>{{ driverStat.miles || '-' }}</span> miles
          </div>
          <div>
            <i class="icofont-speed-meter"></i>
            <span>{{ driverStat.milesPerHour || '-' }}</span> mph
          </div>
        </div>
      </article>
      <div style="text-align: center;">
        <a class="button success" [routerLink]="['/']">
          See others
        </a>
      </div>
    </section>
  `,
  styles: [`
    article {
      padding: 1rem;
      max-width: 800px;
      justify-content: center;
    }

    article > div > div {
      text-align: center;
    }

    article > div > div i {
      margin-right: 0.5rem;
    }

    article > div > div:first-child {
      width: 100%;
    }

    section {
      display: flex;
      flex-direction: column;
      margin: 0 auto;
      max-width: 800px;
    }

    .error {
      background: #ff4136;
      color: white;
      padding: 1rem;
      border-radius: 5px;
    }

    .info {
      background: #0074d9;
      color: white;
      padding: 1rem;
      border-radius: 5px;
    }
  `]
})
export class TripSummaryComponent implements OnInit {

  error: string = '';
  data: any;
  info: string = '';
  processId: string = '';
  repeated: boolean = false;

  constructor(private route: ActivatedRoute, private importService: BatchProcessorService) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.processId = params['id'];
      this.getTripSummary();
      this.repeated = true;
    });
  }

  getTripSummary() {

    this.error = '';
    this.data = [];
    this.info = '';

    this.importService.getInfo(this.processId)
      .subscribe((result: any) => {
        if (result.status === "success") {
          if (typeof result.data === "string") {
            this.info = result.data;

            if (!this.repeated) {
              setTimeout(() => {
                this.getTripSummary()
              }, 4000);
            }
            
          } else {
            this.data = result.data;
          }
        } else {
          this.error = result.message ?? "Error when downloading trip information.";
        }
      });
  }
}
