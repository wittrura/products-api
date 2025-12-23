import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Product as ProductService } from '../product';
import { Product as ProductModel } from '../product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.html',
})
export class ProductList implements OnInit {
  products = signal(<ProductModel[]>[]);
  loading = signal(true);
  error = signal<string | null>(null);

  productService = inject(ProductService)

  ngOnInit(): void {
    this.productService.getProducts().subscribe({
      next: (items) => {
        this.products.set(items);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load products. Please try again.');
        this.loading.set(false);
        console.error(err);
      }
    });
  }
}