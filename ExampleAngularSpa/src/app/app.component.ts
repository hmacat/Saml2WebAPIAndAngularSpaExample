import { Component, OnInit } from '@angular/core';
import { environment } from '@app-env/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router, ActivationEnd } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  ssoSignOnUrl = environment.samlSsoUrl;
  ssoSignOutUrl = environment.samlSloUrl;

  isSignedIn = false;
  apiResult: string;
  token: string;  // JWT
  fullName: string;
  jwtString: string; // To display JWT

  // Material progress spinner
  color = 'primary';
  mode = 'indeterminate';
  value = 60;
  isSpinnerVisible = false;

  constructor(private http: HttpClient, private router: Router, private auth: AuthService) {

    // Clear an existing token
    localStorage.removeItem('token');

    // Subscribe to router events, capture token at ActivationEnd
    this.router.events.subscribe(event => {

      if (event instanceof ActivationEnd) {

        // Get JWT from SAML Authorization
        this.auth.retrieveJwt()
        .subscribe((jwt) => {

          // Save jwt to browser storage
          localStorage.setItem('token', jwt);

          // Set flag
          this.isSignedIn = true;

          // Decode the token, assign full name property
          const helper = new JwtHelperService();
          const decodedToken = helper.decodeToken(jwt);
          this.fullName = `${decodedToken.given_name} ${decodedToken.family_name}`;
          this.jwtString = JSON.stringify(decodedToken);

          // For info/debug
          console.log(`JWT:\n${jwt}\nDecoded JWT:\n${this.jwtString}`);

        }, (err) => {

          // Set flag
          this.isSignedIn = false;

          // For info/debug
          console.log('Error fetching JWT: ', err);
        });
      }

    });

  }


  // Button click event handler to call WebAPI
  onClick() {

    this.isSpinnerVisible = true;

    const url = `${environment.apiUrl}/Authorization/TestAuthorization`;

    this.apiResult = `Calling api "${url}" with JWT...`;

    setTimeout( () => {

        this.http.get<any>(url)
          .subscribe(
            data => {
              this.isSpinnerVisible = false;
              this.apiResult = data;
            },
            err => this.apiResult = err.message
          );
      }
      , 2000);
  }


}
