# API de Categorias - Documentação para Frontend

## Base URL
```
https://api.stockmind.com/api/categories
```

## Autenticação
Todos os endpoints requerem autenticação via token JWT no header:
```
Authorization: Bearer {seu-token-jwt}
```

---

## Endpoints Disponíveis

### 1. Listar Todas as Categorias
**GET** `/api/categories`

Lista todas as categorias cadastradas no sistema.

#### Permissões
- ? Admin
- ? Manager
- ? Operator
- ? Viewer

#### Request
```http
GET /api/categories HTTP/1.1
Host: api.stockmind.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response Success (200 OK)
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Eletrônicos",
    "description": "Produtos eletrônicos em geral",
    "isActive": true,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-20T14:45:00Z"
  },
  {
    "id": "7ba85f64-5717-4562-b3fc-2c963f66afa7",
    "name": "Alimentos",
    "description": "Produtos alimentícios",
    "isActive": true,
    "createdAt": "2024-01-16T08:20:00Z",
    "updatedAt": null
  }
]
```

#### Response Error (400 Bad Request)
```json
{
  "error": "Mensagem de erro específica"
}
```

---

### 2. Buscar Categoria por ID
**GET** `/api/categories/{id}`

Busca uma categoria específica pelo ID.

#### Permissões
- ? Admin
- ? Manager
- ? Operator
- ? Viewer

#### Request
```http
GET /api/categories/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Host: api.stockmind.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response Success (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Eletrônicos",
  "description": "Produtos eletrônicos em geral",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-20T14:45:00Z"
}
```

#### Response Error (404 Not Found)
```json
{
  "error": "Category not found"
}
```

---

### 3. Criar Nova Categoria
**POST** `/api/categories`

Cria uma nova categoria no sistema.

#### Permissões
- ? Admin
- ? Manager
- ? Operator
- ? Viewer

#### Request
```http
POST /api/categories HTTP/1.1
Host: api.stockmind.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "Vestuário",
  "description": "Roupas e acessórios"
}
```

#### Request Body Schema
```typescript
{
  name: string;        // Obrigatório - Nome da categoria
  description: string; // Obrigatório - Descrição da categoria
}
```

#### Response Success (201 Created)
```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afa8"
}
```

**Headers:**
```
Location: /api/categories
```

#### Response Error (400 Bad Request)
```json
{
  "error": "Nome da categoria já existe"
}
```

#### Response Error (401 Unauthorized)
```json
{
  "error": "Token inválido ou expirado"
}
```

#### Response Error (403 Forbidden)
```json
{
  "error": "Usuário não tem permissão para criar categorias"
}
```

---

### 4. Atualizar Categoria Existente
**PUT** `/api/categories/{id}`

Atualiza uma categoria existente.

#### Permissões
- ? Admin
- ? Manager
- ? Operator
- ? Viewer

#### Request
```http
PUT /api/categories/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Host: api.stockmind.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Eletrônicos e Informática",
  "description": "Produtos eletrônicos, computadores e acessórios"
}
```

#### Request Body Schema
```typescript
{
  id: string;          // Obrigatório - UUID da categoria (deve corresponder ao ID na URL)
  name: string;        // Obrigatório - Novo nome da categoria
  description: string; // Obrigatório - Nova descrição da categoria
}
```

#### Response Success (204 No Content)
Sem corpo de resposta.

#### Response Error (400 Bad Request)
```json
{
  "error": "ID mismatch"
}
```
ou
```json
{
  "error": "Category name already exists"
}
```

#### Response Error (404 Not Found)
```json
{
  "error": "Category not found"
}
```

---

### 5. Deletar Categoria (Soft Delete)
**DELETE** `/api/categories/{id}`

Desativa uma categoria (soft delete). A categoria não é removida do banco de dados, apenas marcada como inativa.

#### Permissões
- ? Admin
- ? Manager
- ? Operator
- ? Viewer

#### Request
```http
DELETE /api/categories/3fa85f64-5717-4562-b3fc-2c963f66afa6 HTTP/1.1
Host: api.stockmind.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Response Success (204 No Content)
Sem corpo de resposta.

#### Response Error (400 Bad Request)
```json
{
  "error": "Cannot delete category with existing products. Reassign products first."
}
```

#### Response Error (404 Not Found)
```json
{
  "error": "Category not found"
}
```

---

## Modelos de Dados

### CategoryDto
```typescript
interface CategoryDto {
  id: string;              // UUID da categoria
  name: string;            // Nome da categoria
  description: string;     // Descrição da categoria
  isActive: boolean;       // Status ativo/inativo
  createdAt: string;       // Data de criação (ISO 8601)
  updatedAt: string | null; // Data de última atualização (ISO 8601) ou null
}
```

### CreateCategoryCommand
```typescript
interface CreateCategoryCommand {
  name: string;        // Nome da categoria (obrigatório)
  description: string; // Descrição da categoria (obrigatório)
}
```

### UpdateCategoryCommand
```typescript
interface UpdateCategoryCommand {
  id: string;          // UUID da categoria (obrigatório)
  name: string;        // Nome da categoria (obrigatório)
  description: string; // Descrição da categoria (obrigatório)
}
```

---

## Códigos de Status HTTP

| Código | Descrição |
|--------|-----------|
| 200 | OK - Requisição bem-sucedida |
| 201 | Created - Recurso criado com sucesso |
| 400 | Bad Request - Dados inválidos ou erro de validação |
| 401 | Unauthorized - Token ausente ou inválido |
| 403 | Forbidden - Usuário sem permissão |
| 404 | Not Found - Recurso não encontrado |
| 500 | Internal Server Error - Erro no servidor |

---

## Exemplos de Uso

### JavaScript/TypeScript (Fetch API)

#### Listar Categorias
```typescript
async function getAllCategories(token: string): Promise<CategoryDto[]> {
  const response = await fetch('https://api.stockmind.com/api/categories', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error);
  }

  return await response.json();
}
```

#### Buscar Categoria por ID
```typescript
async function getCategoryById(token: string, id: string): Promise<CategoryDto> {
  const response = await fetch(`https://api.stockmind.com/api/categories/${id}`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error);
  }

  return await response.json();
}
```

#### Criar Categoria
```typescript
async function createCategory(
  token: string, 
  data: CreateCategoryCommand
): Promise<{ id: string }> {
  const response = await fetch('https://api.stockmind.com/api/categories', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error);
  }

  return await response.json();
}

// Uso:
try {
  const newCategory = await createCategory(token, {
    name: 'Móveis',
    description: 'Móveis para casa e escritório'
  });
  console.log('Categoria criada com ID:', newCategory.id);
} catch (error) {
  console.error('Erro ao criar categoria:', error.message);
}
```

#### Atualizar Categoria
```typescript
async function updateCategory(
  token: string,
  id: string,
  data: UpdateCategoryCommand
): Promise<void> {
  const response = await fetch(`https://api.stockmind.com/api/categories/${id}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error);
  }
}

// Uso:
try {
  await updateCategory(token, '3fa85f64-5717-4562-b3fc-2c963f66afa6', {
    id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    name: 'Eletrônicos e Informática',
    description: 'Produtos eletrônicos, computadores e acessórios'
  });
  console.log('Categoria atualizada com sucesso');
} catch (error) {
  console.error('Erro ao atualizar categoria:', error.message);
}
```

#### Deletar Categoria
```typescript
async function deleteCategory(token: string, id: string): Promise<void> {
  const response = await fetch(`https://api.stockmind.com/api/categories/${id}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error);
  }
}

// Uso:
try {
  await deleteCategory(token, '3fa85f64-5717-4562-b3fc-2c963f66afa6');
  console.log('Categoria deletada com sucesso');
} catch (error) {
  console.error('Erro ao deletar categoria:', error.message);
}
```

### Axios

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://api.stockmind.com/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Interceptor para adicionar token
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Listar categorias
export const getCategories = async () => {
  const { data } = await api.get<CategoryDto[]>('/categories');
  return data;
};

// Buscar categoria por ID
export const getCategoryById = async (id: string) => {
  const { data } = await api.get<CategoryDto>(`/categories/${id}`);
  return data;
};

// Criar categoria
export const createCategory = async (category: CreateCategoryCommand) => {
  const { data } = await api.post<{ id: string }>('/categories', category);
  return data;
};

// Atualizar categoria
export const updateCategory = async (id: string, category: UpdateCategoryCommand) => {
  await api.put(`/categories/${id}`, category);
};

// Deletar categoria
export const deleteCategory = async (id: string) => {
  await api.delete(`/categories/${id}`);
};
```

### React Hook Exemplo

```typescript
import { useState, useEffect } from 'react';

interface UseCategories {
  categories: CategoryDto[];
  loading: boolean;
  error: string | null;
  getCategoryById: (id: string) => Promise<CategoryDto | null>;
  createCategory: (data: CreateCategoryCommand) => Promise<void>;
  updateCategory: (id: string, data: UpdateCategoryCommand) => Promise<void>;
  deleteCategory: (id: string) => Promise<void>;
  refreshCategories: () => Promise<void>;
}

export function useCategories(): UseCategories {
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchCategories = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const token = localStorage.getItem('token');
      const response = await fetch('https://api.stockmind.com/api/categories', {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        throw new Error('Erro ao carregar categorias');
      }

      const data = await response.json();
      setCategories(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido');
    } finally {
      setLoading(false);
    }
  };

  const getCategoryById = async (id: string): Promise<CategoryDto | null> => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`https://api.stockmind.com/api/categories/${id}`, {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        throw new Error('Categoria não encontrada');
      }

      return await response.json();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido');
      return null;
    }
  };

  const createCategory = async (data: CreateCategoryCommand) => {
    const token = localStorage.getItem('token');
    const response = await fetch('https://api.stockmind.com/api/categories', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error);
    }

    await fetchCategories(); // Recarrega a lista
  };

  const updateCategory = async (id: string, data: UpdateCategoryCommand) => {
    const token = localStorage.getItem('token');
    const response = await fetch(`https://api.stockmind.com/api/categories/${id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error);
    }

    await fetchCategories(); // Recarrega a lista
  };

  const deleteCategory = async (id: string) => {
    const token = localStorage.getItem('token');
    const response = await fetch(`https://api.stockmind.com/api/categories/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error);
    }

    await fetchCategories(); // Recarrega a lista
  };

  useEffect(() => {
    fetchCategories();
  }, []);

  return {
    categories,
    loading,
    error,
    getCategoryById,
    createCategory,
    updateCategory,
    deleteCategory,
    refreshCategories: fetchCategories
  };
}
```

---

## Validações

### Campo `name`
- ? Obrigatório
- ? Deve ser único (não pode ter duas categorias com o mesmo nome)
- ? Tamanho máximo: 100 caracteres
- ? Não pode ser vazio ou apenas espaços em branco

### Campo `description`
- ? Obrigatório
- ? Pode ser uma string vazia

---

## Notas Importantes

1. **Autenticação**: Todos os endpoints requerem autenticação. Sem token válido, você receberá `401 Unauthorized`.

2. **Permissões**: 
   - **Visualizar**: Todos os papéis (Admin, Manager, Operator, Viewer)
   - **Criar/Atualizar/Deletar**: Apenas Admin e Manager

3. **Formato de Data**: Todas as datas são retornadas no formato ISO 8601 (UTC).

4. **UUIDs**: Todos os IDs são UUIDs no formato `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`.

5. **Soft Delete**: Categorias não são removidas fisicamente do banco de dados. Ao deletar, o campo `isActive` é marcado como `false`.

6. **Regras de Negócio**:
   - Não é possível deletar uma categoria que possui produtos associados
   - Nomes de categorias devem ser únicos no sistema

---

## Suporte

Para dúvidas ou problemas, entre em contato com a equipe de backend ou consulte a documentação completa do Swagger em:
```
https://api.stockmind.com/swagger
```
