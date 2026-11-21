import { CommonModule, DatePipe, registerLocaleData } from '@angular/common';
import localeEs from '@angular/common/locales/es';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import {
  ListService,
  LocalizationPipe,
  LocalizationService,
  PagedAndSortedResultRequestDto,
  PagedResultDto,
} from '@abp/ng.core';
import { Confirmation, ConfirmationService, ModalComponent } from '@abp/ng.theme.shared';
import { catchError, finalize, of } from 'rxjs';
import {
  PantryItemDto,
  PantryItemService,
  PantryItemStatus,
  UpdatePantryItemDto,
} from '../proxy/pantry';

registerLocaleData(localeEs);

function validDateValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) {
    return null;
  }
  if (typeof value !== 'string' || !/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    return { invalidDate: true };
  }
  const [year, month, day] = value.split('-').map(Number);
  const date = new Date(Date.UTC(year, month - 1, day));
  const isValidCalendarDate =
    date.getUTCFullYear() === year &&
    date.getUTCMonth() === month - 1 &&
    date.getUTCDate() === day;

  return isValidCalendarDate ? null : { invalidDate: true };
}

@Component({
  selector: 'app-pantry',
  standalone: true,
  imports: [CommonModule, DatePipe, ReactiveFormsModule, LocalizationPipe, ModalComponent],
  providers: [ListService],
  templateUrl: './pantry.component.html',
  styleUrl: './pantry.component.scss',
})
export class PantryComponent implements OnInit {
  readonly list = inject(ListService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly pantryItemService = inject(PantryItemService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly localization = inject(LocalizationService);
  private readonly fb = inject(FormBuilder);

  readonly PantryItemStatus = PantryItemStatus;

  items: PantryItemDto[] = [];
  totalCount = 0;

  isLoading = true;
  hasError = false;
  errorKey = '::Pantry:LoadError';

  statusFilter: 'all' | 'available' | 'consumed' = 'all';

  // Estado del modal de edición
  isEditModalOpen = false;
  isSaving = false;
  saveErrorMessage: string | null = null;
  saveErrorIsKey = true;
  editingItemId: string | null = null;
  editingProductLabel = '';

  editForm: FormGroup = this.fb.group({
    quantity: [null, [Validators.min(0)]],
    unit: ['', [Validators.maxLength(32)]],
    expirationDate: ['', [validDateValidator]],
  });

  get currentLocale(): string {
    const lang = this.localization.currentLang || 'es';
    return lang.startsWith('es') ? 'es' : 'en-US';
  }

  get filteredItems(): PantryItemDto[] {
    if (this.statusFilter === 'available') {
      return this.items.filter(i => i.status === PantryItemStatus.Available);
    }
    if (this.statusFilter === 'consumed') {
      return this.items.filter(i => i.status === PantryItemStatus.Consumed);
    }
    return this.items;
  }

  get totalPages(): number {
    const pageSize = this.list.maxResultCount || 10;
    return Math.max(1, Math.ceil(this.totalCount / pageSize));
  }

  ngOnInit(): void {
    const streamCreator = (query: PagedAndSortedResultRequestDto) => {
      this.isLoading = true;
      this.hasError = false;
      this.cdr.markForCheck();

      return this.pantryItemService.getList(query, { skipHandleError: true }).pipe(
        catchError(() => {
          this.hasError = true;
          this.errorKey = '::Pantry:LoadError';
          return of({ items: [], totalCount: 0 } as PagedResultDto<PantryItemDto>);
        }),
        finalize(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        })
      );
    };

    this.list.hookToQuery(streamCreator).subscribe(response => {
      this.items = response.items ?? [];
      this.totalCount = response.totalCount ?? 0;
      this.cdr.markForCheck();
    });
  }

  retry(): void {
    this.list.get();
  }

  setFilter(filter: 'all' | 'available' | 'consumed'): void {
    this.statusFilter = filter;
  }

  openEditModal(item: PantryItemDto): void {
    if (!item.id) {
      return;
    }

    this.editingItemId = item.id;
    this.editingProductLabel = item.productName || item.productBarcode || '';
    this.saveErrorMessage = null;
    this.saveErrorIsKey = true;

    const formattedDate = item.expirationDate ? item.expirationDate.substring(0, 10) : '';

    this.editForm.reset({
      quantity: item.quantity ?? null,
      unit: item.unit ?? '',
      expirationDate: formattedDate,
    });

    this.isEditModalOpen = true;
  }

  closeEditModal(): void {
    if (this.isSaving) {
      return;
    }
    this.isEditModalOpen = false;
    this.editingItemId = null;
    this.saveErrorMessage = null;
  }

  saveEdit(): void {
    if (this.editForm.invalid || !this.editingItemId || this.isSaving) {
      this.editForm.markAllAsTouched();
      return;
    }

    const raw = this.editForm.value;
    const dto: UpdatePantryItemDto = {
      quantity:
        raw.quantity === '' || raw.quantity === null || raw.quantity === undefined
          ? null
          : Number(raw.quantity),
      unit: raw.unit && raw.unit.trim().length > 0 ? raw.unit.trim() : null,
      expirationDate: raw.expirationDate
        ? new Date(`${raw.expirationDate}T00:00:00Z`).toISOString()
        : null,
    };

    this.isSaving = true;
    this.saveErrorMessage = null;
    this.cdr.markForCheck();

    this.pantryItemService
      .update(this.editingItemId, dto, { skipHandleError: true })
      .pipe(
        finalize(() => {
          this.isSaving = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.isEditModalOpen = false;
          this.editingItemId = null;
          this.cdr.markForCheck();
          this.list.get();
        },
        error: (err: any) => {
          const serverMessage = err?.error?.error?.message;
          if (serverMessage) {
            this.saveErrorMessage = serverMessage;
            this.saveErrorIsKey = false;
          } else {
            this.saveErrorMessage = '::Pantry:SaveError';
            this.saveErrorIsKey = true;
          }
          this.cdr.markForCheck();
        },
      });
  }

  markAsConsumed(item: PantryItemDto): void {
    if (!item.id || item.status === PantryItemStatus.Consumed) {
      return;
    }

    this.pantryItemService.markAsConsumed(item.id).subscribe(() => {
      this.list.get();
    });
  }

  deleteItem(item: PantryItemDto): void {
    if (!item.id) {
      return;
    }

    this.confirmation
      .warn('::Pantry:DeleteConfirmationMessage', '::Pantry:DeleteConfirmationTitle')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm && item.id) {
          this.pantryItemService.delete(item.id).subscribe(() => {
            this.list.get();
          });
        }
      });
  }

  previousPage(): void {
    if (this.list.page > 0) {
      this.list.page = this.list.page - 1;
    }
  }

  nextPage(): void {
    if (this.list.page + 1 < this.totalPages) {
      this.list.page = this.list.page + 1;
    }
  }
}
