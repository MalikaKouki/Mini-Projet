import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommercialApiService } from './commercial-api.service';
import type {
  Client,
  ClientWriteRequest,
  Order,
  OrderWriteRequest,
  Product,
  ProductWriteRequest,
} from './commercial-api.service';

type Section = 'clients' | 'products' | 'orders';
type OrderLineInput = { productId: number | null; quantity: number | null };

@Component({
  imports: [CommonModule, FormsModule],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App implements OnInit {
  section: Section = 'clients';
  clients: Client[] = [];
  products: Product[] = [];
  orders: Order[] = [];
  errorMessage = '';
  successMessage = '';
  editingClientId: number | null = null;
  editingProductId: number | null = null;
  editingOrderId: number | null = null;
  editingOrderStatus: string | null = null;
  private editingOrderOriginalLines: Order['lines'] = [];
  selectedOrder: Order | null = null;

  clientForm: ClientWriteRequest = this.emptyClientForm();
  productForm: ProductWriteRequest = this.emptyProductForm();
  orderForm: { clientId: number | null; lines: OrderLineInput[] } = this.emptyOrderForm();

  constructor(private readonly api: CommercialApiService) {}

  ngOnInit(): void {
    this.loadClients();
    this.loadProducts();
    this.loadOrders();
  }

  selectSection(section: Section): void {
    this.section = section;
    this.clearMessages();
  }

  saveClient(): void {
    this.clearMessages();
    const request = { ...this.clientForm };
    const operation = this.editingClientId === null
      ? this.api.createClient(request)
      : this.api.updateClient(this.editingClientId, request);

    operation.subscribe({
      next: () => {
        this.successMessage = this.editingClientId === null
          ? 'Client créé.'
          : 'Client modifié.';
        this.cancelClientEdit();
        this.loadClients();
      },
      error: (error: unknown) => this.showError(error, 'Impossible d’enregistrer le client.'),
    });
  }

  editClient(client: Client): void {
    this.editingClientId = client.id;
    this.clientForm = {
      lastName: client.lastName,
      firstName: client.firstName ?? '',
      email: client.email ?? '',
      phone: client.phone ?? '',
      address: client.address ?? '',
    };
    this.clearMessages();
  }

  cancelClientEdit(): void {
    this.editingClientId = null;
    this.clientForm = this.emptyClientForm();
  }

  deleteClient(client: Client): void {
    if (!window.confirm(`Supprimer le client ${client.lastName} ?`)) {
      return;
    }

    this.clearMessages();
    this.api.deleteClient(client.id).subscribe({
      next: () => {
        this.successMessage = 'Client supprimé.';
        this.loadClients();
      },
      error: (error: unknown) => this.showError(error, 'Impossible de supprimer le client.'),
    });
  }

  saveProduct(): void {
    this.clearMessages();
    const request = {
      ...this.productForm,
      reference: this.productForm.reference.trim(),
      name: this.productForm.name.trim(),
      description: this.productForm.description.trim(),
    };

    if (!request.reference || !request.name) {
      this.errorMessage = 'La référence et le nom du produit sont obligatoires.';
      return;
    }

    if (
      request.reference.length > 50 ||
      request.name.length > 150 ||
      request.description.length > 1000
    ) {
      this.errorMessage = 'Un champ dépasse la longueur maximale autorisée.';
      return;
    }

    if (!Number.isFinite(request.unitPrice) || request.unitPrice < 0) {
      this.errorMessage = 'Le prix unitaire doit être un nombre positif ou nul.';
      return;
    }

    if (!Number.isInteger(request.stockQuantity) || request.stockQuantity < 0) {
      this.errorMessage = 'Le stock doit être un nombre entier positif ou nul.';
      return;
    }

    const operation = this.editingProductId === null
      ? this.api.createProduct(request)
      : this.api.updateProduct(this.editingProductId, request);

    operation.subscribe({
      next: () => {
        this.successMessage = this.editingProductId === null
          ? 'Produit créé.'
          : 'Produit modifié.';
        this.cancelProductEdit();
        this.loadProducts();
      },
      error: (error: unknown) => this.showError(error, 'Impossible d’enregistrer le produit.'),
    });
  }

  editProduct(product: Product): void {
    this.editingProductId = product.id;
    this.productForm = {
      reference: product.reference,
      name: product.name,
      description: product.description ?? '',
      unitPrice: product.unitPrice,
      stockQuantity: product.stockQuantity,
    };
    this.clearMessages();
  }

  cancelProductEdit(): void {
    this.editingProductId = null;
    this.productForm = this.emptyProductForm();
  }

  deleteProduct(product: Product): void {
    if (!window.confirm(`Supprimer le produit ${product.name} ?`)) {
      return;
    }

    this.clearMessages();
    this.api.deleteProduct(product.id).subscribe({
      next: () => {
        this.successMessage = 'Produit supprimé.';
        this.loadProducts();
      },
      error: (error: unknown) => this.showError(error, 'Impossible de supprimer le produit.'),
    });
  }

  addOrderLine(): void {
    this.orderForm.lines = [...this.orderForm.lines, { productId: null, quantity: null }];
  }

  removeOrderLine(index: number): void {
    if (this.orderForm.lines.length > 1) {
      this.orderForm.lines = this.orderForm.lines.filter((_, lineIndex) => lineIndex !== index);
    }
  }

  productForLine(productId: number | null): Product | undefined {
    return this.products.find((product) => product.id === Number(productId));
  }

  availableStock(productId: number): number {
    const stock = this.products.find((product) => product.id === productId)?.stockQuantity ?? 0;
    if (this.editingOrderStatus !== 'Validée') {
      return stock;
    }

    const quantityInCurrentOrder = this.editingOrderOriginalLines
      .filter((line) => line.productId === productId)
      .reduce((total, line) => total + line.quantity, 0);

    return stock + quantityInCurrentOrder;
  }

  lineTotal(line: OrderLineInput): number {
    return (this.productForLine(line.productId)?.unitPrice ?? 0) * (line.quantity ?? 0);
  }

  orderTotalHT(): number {
    return this.orderForm.lines.reduce((total, line) => total + this.lineTotal(line), 0);
  }

  hasCalculatedTotals(): boolean {
    return this.orderForm.lines.some(
      (line) => line.productId !== null && line.quantity !== null && line.quantity > 0,
    );
  }

  saveOrder(): void {
    this.clearMessages();
    const lines = this.orderForm.lines
      .filter(
        (line): line is { productId: number; quantity: number } =>
          line.productId !== null &&
          Number.isInteger(Number(line.productId)) &&
          line.quantity !== null &&
          Number.isInteger(line.quantity) &&
          line.quantity > 0,
      )
      .map((line) => ({ productId: Number(line.productId), quantity: line.quantity }));

    if (this.orderForm.clientId === null) {
      this.errorMessage = 'Sélectionne un client.';
      return;
    }

    if (lines.length !== this.orderForm.lines.length || lines.length === 0) {
      this.errorMessage = 'Ajoute au moins une ligne avec un produit et une quantité positive.';
      return;
    }

    if (new Set(lines.map((line) => line.productId)).size !== lines.length) {
      this.errorMessage = 'Un même produit ne peut apparaître qu’une fois dans la commande.';
      return;
    }

    const unavailableLine = lines.find((line) => {
      const product = this.products.find((item) => item.id === line.productId);
      return !product || line.quantity > this.availableStock(line.productId);
    });

    if (unavailableLine) {
      this.errorMessage = 'La quantité commandée dépasse le stock disponible.';
      return;
    }

    const request: OrderWriteRequest = {
      clientId: Number(this.orderForm.clientId),
      lines,
    };
    const operation = this.editingOrderId === null
      ? this.api.createOrder(request)
      : this.api.updateOrder(this.editingOrderId, request);

    operation.subscribe({
      next: () => {
        this.successMessage = this.editingOrderId === null
          ? 'Commande créée en brouillon.'
          : 'Commande modifiée.';
        this.cancelOrderEdit();
        this.loadOrders();
      },
      error: (error: unknown) => this.showError(error, 'Impossible d’enregistrer la commande.'),
    });
  }

  editOrder(order: Order): void {
    this.editingOrderId = order.id;
    this.editingOrderStatus = order.status;
    this.editingOrderOriginalLines = order.lines.map((line) => ({ ...line }));
    this.orderForm = {
      clientId: order.clientId,
      lines: order.lines.map((line) => ({
        productId: line.productId,
        quantity: line.quantity,
      })),
    };
    this.selectedOrder = null;
    this.clearMessages();
  }

  cancelOrderEdit(): void {
    this.editingOrderId = null;
    this.editingOrderStatus = null;
    this.editingOrderOriginalLines = [];
    this.orderForm = this.emptyOrderForm();
  }

  validateOrder(order: Order): void {
    this.clearMessages();
    this.api.validateOrder(order.id).subscribe({
      next: (validatedOrder) => {
        this.successMessage = `Commande ${order.orderNumber} validée et stock mis à jour.`;
        this.selectedOrder = validatedOrder;
        this.loadOrders();
        this.loadProducts();
      },
      error: (error: unknown) => this.showError(error, 'Impossible de valider la commande.'),
    });
  }

  deleteOrder(order: Order): void {
    if (!window.confirm(`Supprimer la commande ${order.orderNumber} ?`)) {
      return;
    }

    this.clearMessages();
    this.api.deleteOrder(order.id).subscribe({
      next: () => {
        this.successMessage = 'Commande supprimée.';
        if (this.selectedOrder?.id === order.id) {
          this.selectedOrder = null;
        }
        this.loadOrders();
      },
      error: (error: unknown) => this.showError(error, 'Impossible de supprimer la commande.'),
    });
  }

  showOrderDetails(order: Order): void {
    this.selectedOrder = order;
  }

  backToOrders(): void {
    this.selectedOrder = null;
  }

  private loadClients(): void {
    this.api.getClients().subscribe({
      next: (clients) => (this.clients = clients),
      error: (error: unknown) => this.showError(error, 'Impossible de charger les clients.'),
    });
  }

  private loadProducts(): void {
    this.api.getProducts().subscribe({
      next: (products) => (this.products = products),
      error: (error: unknown) => this.showError(error, 'Impossible de charger les produits.'),
    });
  }

  private loadOrders(): void {
    this.api.getOrders().subscribe({
      next: (orders) => (this.orders = orders),
      error: (error: unknown) => this.showError(error, 'Impossible de charger les commandes.'),
    });
  }

  private showError(error: unknown, fallback: string): void {
    if (error instanceof HttpErrorResponse) {
      const body = error.error as { message?: string; title?: string } | string | null;
      if (typeof body === 'string' && body.length > 0) {
        this.errorMessage = body;
        return;
      }
      if (body && typeof body === 'object' && (body.message || body.title)) {
        this.errorMessage = body.message ?? body.title ?? fallback;
        return;
      }
    }
    this.errorMessage = fallback;
  }

  private clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }

  private emptyClientForm(): ClientWriteRequest {
    return { lastName: '', firstName: '', email: '', phone: '', address: '' };
  }

  private emptyProductForm(): ProductWriteRequest {
    return { reference: '', name: '', description: '', unitPrice: 0, stockQuantity: 0 };
  }

  private emptyOrderForm(): { clientId: number | null; lines: OrderLineInput[] } {
    return { clientId: null, lines: [{ productId: null, quantity: null }] };
  }
}
