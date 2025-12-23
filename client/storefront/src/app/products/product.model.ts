export interface Product {
  id: number;
  name: string;
  description?: string | null;
  price: number;
  stockQuantity: number;
  createdDate: string;
  categoryId: number;
  categoryName: string;
}