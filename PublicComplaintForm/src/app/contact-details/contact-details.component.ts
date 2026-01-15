import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormHandlerService } from '../form-handler.service';
import { BreadcrumbsManagerService } from '../breadcrumbs-manager.service';
import { MatSelectModule } from '@angular/material/select';
import { CourtHandlerService } from '../court-handler.service';


@Component({
	selector: 'app-contact-details',
	standalone: true,
	imports: [CommonModule, ReactiveFormsModule, MatSelectModule],
	templateUrl: './contact-details.component.html',
	styleUrl: './contact-details.component.scss'
})
export class ContactDetailsComponent implements OnInit 
{
	form!: FormGroup;
	currentPage = "step3";
	isSubmitting = false;
	textAreaRemainingCharacters: string = "7000 תווים נותרו";
	readonly maxTextAreaLength = 7000;
	
	selectedCourt: string | undefined = undefined;
	courtsList: any;

	constructor(
		private breadcrumbsManagerService: BreadcrumbsManagerService,
		private formBuilder: FormBuilder,
		private router: Router,
		private formHandlerService: FormHandlerService,
		private courtHandlerService: CourtHandlerService
	) {}

	async ngOnInit() 
	{
		this.form = this.formBuilder.group({
			contactDescription: ['', Validators.required],
			courtCaseNumber: ['', Validators.required],
			courthouse: ['', Validators.required]
		});

		(await this.courtHandlerService.getCourtsList()).subscribe({
			next: (data: any) => {
				this.courtsList = data.courtsList;
				console.log(data);
			},
			error: (error: any) => {
				console.log(error);
			},
			complete: () => {}
		});

		this.updateFormGroup();

		this.updateCharacterCounter();
	}

	ngAfterViewInit(): void
	{
		this.breadcrumbsManagerService.setStep(3);
	}

	OnCourtSelectionChanged(event: any)
	{
		this.selectedCourt = event.value;

		this.form.patchValue({
			courthouse: this.selectedCourt
		});

		this.formHandlerService.updateStepFields('3', this.form);
	}

	OnTextAreaChanged(event: Event)
	{
		const textArea = event.target as HTMLTextAreaElement;
		const currentValue = textArea.value;

		if(currentValue.length > this.maxTextAreaLength)
		{
			textArea.value = currentValue.substring(0, this.maxTextAreaLength);
		}

		this.updateCharacterCounter();
	}

	private updateCharacterCounter(): void
	{
		const currentValue = this.form.get('contactDescription')?.value || '';
		const lengthRemaining = Math.max(0, this.maxTextAreaLength - currentValue.length);
		this.textAreaRemainingCharacters = `${lengthRemaining} תווים נותרו`;
	}

	GoToNextStep()
	{
		this.formHandlerService.updateStepFields('3', this.form);

		if(!this.form.valid)
		{
			Object.keys(this.form.controls).forEach(field => {
				const control = this.form.get(field);
				control?.markAsTouched({ onlySelf: true });
			});
			return;
		}

		this.router.navigate(['/step4']);
	}

	GoToPrevPage()
	{
		this.formHandlerService.updateStepFields('3', this.form);
		this.router.navigate(['/step2']);
	}

	updateFormGroup(): void 
	{
		var stepForm = this.formHandlerService.getStepValues('3');

		if(stepForm) {
			Object.keys(stepForm.controls).forEach((controlName) => {
				if(controlName === "courthouse")
				{
					this.selectedCourt = stepForm.get(controlName)?.value;
					this.form.patchValue({
						courthouse: this.selectedCourt
					});
				}
				else if(this.form.contains(controlName))
					this.form?.get(controlName)?.setValue(stepForm.get(controlName)?.value);
			});
		}
	}

	async submitContactDetails()
	{
		this.isSubmitting = true;
		if(!this.form.valid)
		{
			Object.keys(this.form.controls).forEach(field => {
				const control = this.form.get(field);
				control?.markAsTouched({ onlySelf: true });
			});
			this.isSubmitting = false;
			return;
		}

		this.GoToNextStep();
		this.isSubmitting = false;
	}
}
