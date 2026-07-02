import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PresentacionComponent } from './presentacion.component';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PresentacionService } from '../../core/services/presentacion.service';
import { of } from 'rxjs';

describe('PresentacionComponent', () => {
  let component: PresentacionComponent;
  let fixture: ComponentFixture<PresentacionComponent>;
  let mockService: jasmine.SpyObj<PresentacionService>;

  beforeEach(async () => {
    mockService = jasmine.createSpyObj('PresentacionService', ['getAll', 'create', 'update', 'delete']);

    await TestBed.configureTestingModule({
      imports: [PresentacionComponent, ReactiveFormsModule, FormsModule, CommonModule],
      providers: [
        { provide: PresentacionService, useValue: mockService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PresentacionComponent);
    component = fixture.componentInstance;

    mockService.getAll.and.returnValue(Promise.resolve([]));
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should call getAll on init', () => {
    expect(mockService.getAll).toHaveBeenCalled();
  });

  it('should initialize the form', () => {
    expect(component.form).toBeTruthy();
    expect(component.form.controls['nombre']).toBeDefined();
  });
});
