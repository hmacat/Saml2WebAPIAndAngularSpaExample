import { JwtHelperService } from '@auth0/angular-jwt';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '@app-env/environment';


@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(public jwtHelper: JwtHelperService, private http: HttpClient) { }

  public getToken(): string {
    return localStorage.getItem('token');
  }

  public isAuthenticated(): boolean {
    // get the token
    const token = this.getToken();
    // return a boolean reflecting
    // whether or not the token is expired
    return this.jwtHelper.isTokenExpired(token);

  }

  // Get JWT from server
  public retrieveJwt() {
    return this.http.get(environment.samlAuthUrl, { withCredentials: true, responseType: 'text' });
  }

}
