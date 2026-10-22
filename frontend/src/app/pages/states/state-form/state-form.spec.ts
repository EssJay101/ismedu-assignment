import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StateForm } from './state-form';

describe('StateForm', () => {
  let component: StateForm;
  let fixture: ComponentFixture<StateForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StateForm],
    }).compileComponents();

    fixture = TestBed.createComponent(StateForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
