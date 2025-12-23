import { Injectable, inject} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product as ProductModel } from './product.model';

@Injectable({providedIn: 'root'})
export class Product {
  private http = inject(HttpClient)

  getProducts(): Observable<ProductModel[]> {
    return this.http.get<ProductModel[]>('/api/products');
  }
}