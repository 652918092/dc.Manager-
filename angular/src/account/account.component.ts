import { Component, ViewContainerRef, OnInit, ViewEncapsulation, Injector } from '@angular/core';
import { LoginService } from './login/login.service';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
    templateUrl: './account.component.html',
    styleUrls: [
        './account.component.less'
    ],
    encapsulation: ViewEncapsulation.None
})
export class AccountComponent extends AppComponentBase implements OnInit {

    versionText: string;
    currentYear: number;

    private viewContainerRef: ViewContainerRef;

    public constructor(
        injector: Injector,
        private _loginService: LoginService
    ) {
        super(injector);
        this.changeLanguage('zh-Hans');
        this.currentYear = new Date().getFullYear();
        this.versionText = this.appSession.application.version + ' [' + this.appSession.application.releaseDate.format('YYYYDDMM') + ']';
    }

    showTenantChange(): boolean {
        return abp.multiTenancy.isEnabled;
    }
    ///// 20191002添加加载语言
    changeLanguage(languageName: string): void {
        abp.utils.setCookieValue(
            'Abp.Localization.CultureName',
            languageName,
            new Date(new Date().getTime() + 5 * 365 * 86400000), // 5 year
            abp.appPath
        );
    }
    ngOnInit(): void {
        $('body').attr('class', 'login-page');
    }
}
