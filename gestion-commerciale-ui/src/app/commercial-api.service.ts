import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import type { Observable } from 'rxjs';

export interface Client {
  id: number;
  lastName: string;
  firstName: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
  createdAt: string;
}

export interface ClientWriteRequest {
  lastName: string;
  firstName: string;
  email: string;
  phone: string;
  address: string;
}

export interface Product {
  id: number;
  reference: string;
  name: string;
  description: string | null;
  unitPrice: number;
  stockQuantity: number;
  createdAt: string;
}

export interface ProductWriteRequest {
  reference: string;
  name: string;
  description: string;
  unitPrice: number;
  stockQuantity: number;
}

export interface OrderLine {
  productId: number;
  productReference: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  clientId: number;
  clientName: string;
  orderDate: string;
  status: string;
  totalHT: number;
  totalTTC: number;
  lines: OrderLine[];
}

export interface OrderWriteRequest {
  clientId: number;
  lines: { productId: number; quantity: number }[];
}

@Injectable({ providedIn: 'root' })
export class CommercialApiService {
  private readonly baseUrl = 'http://localhost:5144/api';

  constructor(private readonly http: HttpClient) {}

  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(`${this.baseUrl}/clients`);
  }

  createClient(request: ClientWriteRequest): Observable<Client> {
    return this.http.post<Client>(`${this.baseUrl}/clients`, request);
  }

  updateClient(id: number, request: ClientWriteRequest): Observable<Client> {
    return this.http.put<Client>(`${this.baseUrl}/clients/${id}`, request);
  }

  deleteClient(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/clients/${id}`);
  }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products`);
  }

  createProduct(request: ProductWriteRequest): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products`, request);
  }

  updateProduct(id: number, request: ProductWriteRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/products/${id}`, request);
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${id}`);
  }

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.baseUrl}/orders`);
  }

  createOrder(request: OrderWriteRequest): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/orders`, request);
  }

  updateOrder(id: number, request: OrderWriteRequest): Observable<Order> {
    return this.http.put<Order>(`${this.baseUrl}/orders/${id}`, request);
  }

  deleteOrder(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/orders/${id}`);
  }

  validateOrder(id: number): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/orders/${id}/validate`, {});
  }

}
