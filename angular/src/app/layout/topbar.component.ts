import { Component, Injector, OnInit, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { AppAuthService } from '@shared/auth/app-auth.service';

@Component({
    templateUrl: './topbar.component.html',
    selector: 'top-bar',
    styleUrls: ['./topbar.component.less'],
    encapsulation: ViewEncapsulation.None
})
export class TopBarComponent extends AppComponentBase implements OnInit {
    shownLoginName = '';
    profilePicture = '/assets/images/default-profile-picture.png';
    constructor(
        injector: Injector,
        private _authService: AppAuthService
    ) {
        super(injector);
    }
    ngOnInit() {
        this.shownLoginName = this.appSession.getShownLoginName();
    }
    logout(): void {
        this._authService.logout();
    }
}
