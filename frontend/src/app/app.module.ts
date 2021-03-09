import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BatchListComponent } from './components/batch-list/batch-list.component';
import { FileUploaderComponent } from './components/file-uploader/file-uploader.component';
import { HttpClientModule } from '@angular/common/http';
import { TripSummaryComponent } from './components/trip-summary/trip-summary.component';

@NgModule({
  declarations: [
    AppComponent,
    BatchListComponent,
    FileUploaderComponent,
    TripSummaryComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
