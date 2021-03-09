import { Component } from '@angular/core';

@Component({
  selector: 'SA-root',
  templateUrl: './app.template.html',
  styleUrls: ['./app.styles.sass']
})
export class AppComponent {
  title = 'sa-app';
  
  toggleSpinner(show: boolean) {
    let mask = document.querySelector('.loader-mask') as HTMLElement;
    mask.style.display = show ? 'flex' : 'none';
  }
}
