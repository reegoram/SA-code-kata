import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BatchListComponent } from './components/batch-list/batch-list.component';
import { TripSummaryComponent } from './components/trip-summary/trip-summary.component';

const routes: Routes = [
  { path: '', component: BatchListComponent },
  { path: ':id', component: TripSummaryComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
