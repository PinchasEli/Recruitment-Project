import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';

@Injectable({
	providedIn: 'root'
})
export class ApiService 
{
	constructor(
		private http: HttpClient,
		private configService: ConfigService
	) {}

	submitReferenceDetails(data: { caseId: string; contact: string; contactFormId: string }): Observable<any>
	{
		const apiUrl = this.configService.getApiUrl();
		const endpoint = `${apiUrl}/contact-form-validation`;

		const headers = new HttpHeaders({
			'Content-Type': 'application/json'
		});

		return this.http.post(endpoint, data, { headers });
	}

	post(endpoint: string, data: any): Observable<any>
	{
		const apiUrl = this.configService.getApiUrl();
		const url = `${apiUrl}${endpoint}`;
		console.log(url, data);

		const headers = new HttpHeaders({
			'Content-Type': 'application/json'
		});

		return this.http.post(url, data, { headers });
	}

}
