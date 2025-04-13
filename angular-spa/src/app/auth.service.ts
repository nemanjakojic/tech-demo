import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';

const AUTHENTICATED = "authenticated";

@Injectable({
  providedIn: 'root',
})
export class AuthService {

  private http = inject(HttpClient);

  private loggedIn = new BehaviorSubject<boolean>(this.hasValidToken());

  login(email: string, password: string) : Observable<{ message: string }> {
    return this.http.post<{ message: string }>('http://localhost:5162/api/Account/login', { 
      email, password 
    })
    .pipe(
      tap((response: any) => {
        localStorage.setItem(AUTHENTICATED, AUTHENTICATED);
        this.loggedIn.next(true);
      })
    );
  }

  logout() {
    localStorage.removeItem(AUTHENTICATED);
  }
  
  isLoggedIn(): Observable<boolean> {
    return this.loggedIn.asObservable();
  }

  private hasValidToken(): boolean {
    const token = localStorage.getItem(AUTHENTICATED);
    // Ideally verify token's validity here (expiration, etc.)
    return !!token;
  }
}
