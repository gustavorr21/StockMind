# Menu and Permissions System

## Overview
StockMind implements a role-based menu and permissions system where each user role (Admin, Manager, Operator, Viewer) has access to specific menu items and features.

## Architecture

### Permission-Based Access Control
- Each menu item requires specific permissions
- Permissions are granted based on user roles
- Frontend receives only the menu items the user can access

### Menu Structure
```
Menu Response
??? Sections (e.g., Dashboard, Products, Stock)
    ??? Items (e.g., List Products, Add Product)
```

---

## API Endpoints

### 1. Get User Menu
Get the complete menu structure for the authenticated user.

```http
GET /api/menu/user-menu
Authorization: Bearer {token}

Response 200:
{
  "sections": [
    {
      "id": "dashboard",
      "title": "Dashboard",
      "icon": "dashboard",
      "route": "/dashboard",
      "order": 1,
      "items": null,
      "requiredPermissions": [1]
    },
    {
      "id": "products",
      "title": "Products",
      "icon": "inventory_2",
      "route": null,
      "order": 2,
      "items": [
        {
          "id": "products-list",
          "title": "All Products",
          "icon": "list",
          "route": "/products",
          "order": 1,
          "requiredPermissions": [10],
          "divider": false
        },
        {
          "id": "products-create",
          "title": "Add Product",
          "icon": "add_circle",
          "route": "/products/create",
          "order": 2,
          "requiredPermissions": [11],
          "divider": false
        }
      ],
      "requiredPermissions": [10]
    }
  ]
}
```

### 2. Get User Permissions
Get all permissions for the authenticated user.

```http
GET /api/menu/user-permissions
Authorization: Bearer {token}

Response 200:
{
  "permissions": [
    { "id": 1, "name": "DashboardView" },
    { "id": 10, "name": "ProductsView" },
    { "id": 11, "name": "ProductsCreate" },
    { "id": 20, "name": "StockView" }
  ]
}
```

---

## Permissions by Role

### Admin (Full Access)
```csharp
- DashboardView
- ProductsView, ProductsCreate, ProductsUpdate, ProductsDelete
- StockView, StockAdd, StockRemove, StockAdjust
- CategoriesView, CategoriesCreate, CategoriesUpdate
- ReportsView, ReportsLowStock, ReportsMovements
- UsersView, UsersCreate, UsersUpdate, UsersDelete, RolesManage
```

### Manager
```csharp
- DashboardView
- ProductsView, ProductsCreate, ProductsUpdate, ProductsDelete
- StockView, StockAdd, StockRemove, StockAdjust
- CategoriesView, CategoriesCreate, CategoriesUpdate
- ReportsView, ReportsLowStock, ReportsMovements
```

### Operator
```csharp
- DashboardView
- ProductsView (read-only)
- StockView, StockAdd, StockRemove
- CategoriesView (read-only)
- ReportsView, ReportsLowStock
```

### Viewer
```csharp
- DashboardView
- ProductsView (read-only)
- StockView (read-only)
- CategoriesView (read-only)
- ReportsView, ReportsLowStock
```

---

## Permission Enum

```csharp
public enum Permission
{
    // Dashboard
    DashboardView = 1,

    // Products
    ProductsView = 10,
    ProductsCreate = 11,
    ProductsUpdate = 12,
    ProductsDelete = 13,

    // Stock
    StockView = 20,
    StockAdd = 21,
    StockRemove = 22,
    StockAdjust = 23,

    // Categories
    CategoriesView = 30,
    CategoriesCreate = 31,
    CategoriesUpdate = 32,

    // Reports
    ReportsView = 40,
    ReportsLowStock = 41,
    ReportsMovements = 42,

    // Administration
    UsersView = 50,
    UsersCreate = 51,
    UsersUpdate = 52,
    UsersDelete = 53,
    RolesManage = 54
}
```

---

## Menu Sections

### Dashboard
- **Icon:** dashboard
- **Route:** /dashboard
- **Required:** DashboardView
- **Roles:** All

### Products
- **Icon:** inventory_2
- **Required:** ProductsView
- **Items:**
  - All Products (/products) - ProductsView
  - Add Product (/products/create) - ProductsCreate
  - Categories (/categories) - CategoriesView

### Stock
- **Icon:** warehouse
- **Required:** StockView
- **Items:**
  - Stock Overview (/stock) - StockView
  - Add Stock (/stock/add) - StockAdd
  - Remove Stock (/stock/remove) - StockRemove
  - Adjust Stock (/stock/adjust) - StockAdjust
  - Stock Movements (/stock/movements) - ReportsMovements

### Reports
- **Icon:** assessment
- **Required:** ReportsView
- **Items:**
  - Low Stock Alert (/reports/low-stock) - ReportsLowStock
  - Stock Movements (/reports/movements) - ReportsMovements

### Administration
- **Icon:** admin_panel_settings
- **Required:** UsersView or RolesManage
- **Roles:** Admin only
- **Items:**
  - Users (/admin/users) - UsersView
  - Roles & Permissions (/admin/roles) - RolesManage

---

## Angular Integration

### 1. Create Menu Service
```typescript
// menu.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';

export interface MenuItem {
  id: string;
  title: string;
  icon: string;
  route: string;
  order: number;
  requiredPermissions: number[];
  divider: boolean;
}

export interface MenuSection {
  id: string;
  title: string;
  icon: string;
  route?: string;
  order: number;
  items?: MenuItem[];
  requiredPermissions?: number[];
}

export interface MenuResponse {
  sections: MenuSection[];
}

export interface Permission {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class MenuService {
  private apiUrl = `${environment.apiUrl}/menu`;

  constructor(private http: HttpClient) {}

  getUserMenu(): Observable<MenuResponse> {
    return this.http.get<MenuResponse>(`${this.apiUrl}/user-menu`);
  }

  getUserPermissions(): Observable<{ permissions: Permission[] }> {
    return this.http.get<{ permissions: Permission[] }>(`${this.apiUrl}/user-permissions`);
  }
}
```

### 2. Create Menu Component
```typescript
// sidebar.component.ts
import { Component, OnInit } from '@angular/core';
import { MenuService, MenuSection } from '@core/services/menu.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent implements OnInit {
  menuSections: MenuSection[] = [];
  loading = true;

  constructor(private menuService: MenuService) {}

  ngOnInit(): void {
    this.loadMenu();
  }

  loadMenu(): void {
    this.menuService.getUserMenu().subscribe({
      next: (response) => {
        this.menuSections = response.sections;
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load menu', error);
        this.loading = false;
      }
    });
  }
}
```

### 3. Menu Template (Angular Material)
```html
<!-- sidebar.component.html -->
<mat-nav-list>
  <ng-container *ngFor="let section of menuSections">
    <!-- Section without items (direct link) -->
    <a mat-list-item 
       *ngIf="!section.items && section.route"
       [routerLink]="section.route"
       routerLinkActive="active">
      <mat-icon matListItemIcon>{{ section.icon }}</mat-icon>
      <span matListItemTitle>{{ section.title }}</span>
    </a>

    <!-- Section with items (expandable) -->
    <mat-expansion-panel *ngIf="section.items">
      <mat-expansion-panel-header>
        <mat-panel-title>
          <mat-icon>{{ section.icon }}</mat-icon>
          <span>{{ section.title }}</span>
        </mat-panel-title>
      </mat-expansion-panel-header>

      <mat-nav-list>
        <a mat-list-item 
           *ngFor="let item of section.items"
           [routerLink]="item.route"
           routerLinkActive="active">
          <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
          <span matListItemTitle>{{ item.title }}</span>
        </a>
      </mat-nav-list>
    </mat-expansion-panel>

    <mat-divider></mat-divider>
  </ng-container>
</mat-nav-list>
```

---

## Permission Directive (Angular)

```typescript
// has-permission.directive.ts
import { Directive, Input, TemplateRef, ViewContainerRef, OnInit } from '@angular/core';
import { MenuService } from '@core/services/menu.service';

@Directive({
  selector: '[appHasPermission]'
})
export class HasPermissionDirective implements OnInit {
  private permissions: number[] = [];

  @Input() set appHasPermission(permission: number | number[]) {
    this.permissions = Array.isArray(permission) ? permission : [permission];
    this.updateView();
  }

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private menuService: MenuService
  ) {}

  ngOnInit(): void {
    this.menuService.getUserPermissions().subscribe({
      next: (response) => {
        const userPermissionIds = response.permissions.map(p => p.id);
        const hasPermission = this.permissions.some(p => userPermissionIds.includes(p));

        if (hasPermission) {
          this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
          this.viewContainer.clear();
        }
      }
    });
  }

  private updateView(): void {
    // Re-check permissions when input changes
  }
}

// Usage:
// <button *appHasPermission="11">Create Product</button>
// <div *appHasPermission="[11, 12]">Admin/Manager Content</div>
```

---

## Testing

### Test Menu Endpoint
```bash
# Login first
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@stockmind.com","password":"Admin@123"}'

# Get menu (use token from login)
curl -X GET https://localhost:7001/api/menu/user-menu \
  -H "Authorization: Bearer {your-token}"

# Get permissions
curl -X GET https://localhost:7001/api/menu/user-permissions \
  -H "Authorization: Bearer {your-token}"
```

### Expected Response (Admin)
```json
{
  "sections": [
    {
      "id": "dashboard",
      "title": "Dashboard",
      "icon": "dashboard",
      "route": "/dashboard",
      "order": 1
    },
    {
      "id": "products",
      "title": "Products",
      "icon": "inventory_2",
      "order": 2,
      "items": [
        {
          "id": "products-list",
          "title": "All Products",
          "icon": "list",
          "route": "/products",
          "order": 1
        },
        {
          "id": "products-create",
          "title": "Add Product",
          "icon": "add_circle",
          "route": "/products/create",
          "order": 2
        }
      ]
    }
  ]
}
```

---

## Benefits

? **Type-Safe** - Enums and strongly-typed DTOs  
? **Zero Database Impact** - All configuration in code  
? **Performance** - No extra database queries  
? **Secure** - Server-side permission filtering  
? **Easy to Maintain** - Single source of truth  
? **Flexible** - Easy to add new menus/permissions  
? **Frontend Ready** - JSON structure ready for Angular Material  

---

## Future Enhancements

- [ ] Add menu caching
- [ ] Support for dynamic menu badges (e.g., "5 low stock items")
- [ ] Menu search functionality
- [ ] Favorite/pinned menu items
- [ ] Recent items tracking
- [ ] Custom menu icons per user
- [ ] Menu analytics (most used items)

---

## Icons Reference (Material Icons)

- dashboard
- inventory_2
- list
- add_circle
- category
- warehouse
- inventory
- add_box
- remove_circle
- tune
- swap_horiz
- assessment
- warning
- timeline
- admin_panel_settings
- people
- security
