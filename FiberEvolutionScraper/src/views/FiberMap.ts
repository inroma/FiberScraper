import 'leaflet/dist/leaflet.css';
import L, { LatLng } from "leaflet";
import { LMap, LTileLayer, LMarker, LLayerGroup, LPopup, LControl, LControlLayers, LIcon } from "vue2-leaflet";
import Vue from 'vue';
import { Action } from 'vuex-class';
import { Component } from 'vue-property-decorator';
import { ToastStoreMethods } from '@/store/ToastStore';
import { ISnackbar, ISnackbarColor } from '@/models/SnackbarInterface';
import FiberPointDTO from '@/models/FiberPointDTO';
import { EtapeFtth } from '@/models/Enums';
import LeafletPopupContent from '@/components/LeafletPopupComponent.vue';
import FiberService from '@/services/FiberService';
import HeaderButtonsComponent from '@/components/HeaderButtonsComponent.vue';

@Component({
    components: {
        'l-map': LMap,
        'l-tile-layer': LTileLayer,
        'l-marker': LMarker,
        'l-popup': LPopup,
        'l-control': LControl,
        'l-layer-group': LLayerGroup,
        'l-icon': LIcon,
        LControlLayers,
        'header-banner-buttons': HeaderButtonsComponent,
        'leaflet-popup-content': LeafletPopupContent
    }
})
export default class FiberMapVue extends Vue {
    //#region Public Properties
    public loading = false;
    public loadingHistory = false;
    public map: null | LMap = null;
    public userLocation = new LatLng(45.76, 4.83);
    public fibers: FiberPointDTO[] = [];
    public zoom = 11;
    public tileProviders = [
        {
          name: 'OpenStreetMap',
          visible: true,
          attribution:
            '&copy; <a target="_blank" href="http://osm.org/copyright">OpenStreetMap</a> contributors',
          url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png'
        },
        {
            name: 'CartoDBDark',
            visible: false,
            url: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
            subdomains: 'abcd'
        },
        {
          name: 'OpenTopoMap',
          visible: false,
          url: 'https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png',
          attribution:
            'Map data: &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)',
        }
    ];
    public bounds!: L.LatLngBounds;
    private openedMarker: null | LMarker = null;
    private date = new Date();
    public recentResult = false;
    public resultFromDb = false;
    public defaultIcon = '/icons/marker-blue.png';
    public redIcon = '/icons/marker-red.png';
    public greenIcon = '/icons/marker-green.png';
    public yellowIcon = '/icons/marker-yellow.png';
    public purpleIcon = '/icons/marker-purple.png';
    public brownIcon = '/icons/marker-orange.png';
    public blackIcon = '/icons/marker-black.png';
    public blueInvertedIcon = '/icons/marker-white.png';
    public layers: { markers: FiberPointDTO[], visible: boolean, name: string }[] = [];
    public icons = [{code: EtapeFtth.ELLIGIBLE, title: "Élligible", icon: this.greenIcon, order: 0},
        {code: EtapeFtth.EN_COURS_IMMEUBLE, title: "Déploiement Immeuble", icon: this.purpleIcon, order: 2},
        {code: EtapeFtth.TERMINE_QUARTIER, title: "Quartier Terminé", icon: this.blueInvertedIcon, order: 3},
        {code: EtapeFtth.EN_COURS_QUARTIER, title: "Déploiement Quartier", icon: this.defaultIcon, order: 4},
        {code: EtapeFtth.PREVU_QUARTIER, title: "Quartier Programmé", icon: this.brownIcon, order: 5},
        {code: EtapeFtth._, title: "Aucune donnée", icon: this.redIcon, order: 6},
        {code: EtapeFtth.PROCHE_CLIENT, title: "Proche Client", icon: this.yellowIcon, order: 1},
        {code: EtapeFtth.UNKNOWN, title: "Statut inconnu", icon: this.blackIcon, order: 7}
    ];
    public openedFiber?: FiberPointDTO = undefined;

    public get headers() {
        return [
            { text: 'Adresse', value: 'libAdresse' },
            { text: 'Création', value: 'created' },
            { text: 'Batiment', value: 'batiment', width: '250px' },
            { text: 'Éligibilité FTTH', value: 'eligibilitesFtth' },
            { text: 'Coord Lat', value: 'x' },
            { text: 'Coord Long', value: 'y' },
        ]
    }

    //#endregion

    @Action(ToastStoreMethods.CREATE_TOAST_MESSAGE)
    private createToast!: (params: ISnackbar) => void;

    public mounted() {
        this.centerMapOnLocation();
    }

    public getCloseAreaFibers() {
        this.recentResult = this.resultFromDb = false;
        this.loading = true;
        this.openedMarker?.mapObject.closePopup();
        this.layers = [];
        FiberService.getCloseAreaFibers(this.userLocation.lat, this.userLocation.lng)
        .then((response) => {
            this.fibers = response.data;
            this.mapFibersToLayer();
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            this.loading = false;
        });
    }

    public updateFibers() {
        this.loading = true;
        FiberService.updateWideArea(this.userLocation.lat, this.userLocation.lng)
        .then((response) => {
            this.createToast({ message: response.data + " points créés / mis à jour." });
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => this.loading = false);
    }

    public getFibers() {
        this.recentResult = this.resultFromDb = false;
        this.loading = true;
        this.openedMarker?.mapObject.closePopup();
        this.layers = [];
        FiberService.getWideArea(this.userLocation.lat, this.userLocation.lng)
        .then((response) => {
            this.fibers = response.data;
            this.mapFibersToLayer();
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            this.loading = false;
        });
    }
    
    public getDbFibers() {
        this.resultFromDb = this.loading = true;
        this.recentResult = false;
        this.openedMarker?.mapObject.closePopup();
        this.layers = [];
        FiberService.getDbFibers(this.userLocation.lat, this.userLocation.lng)
        .then((response) => {
            this.fibers = response.data;
            this.mapFibersToLayer();
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            this.loading = false;
        });
    }

    public getNewestFibers() {
        this.recentResult = this.resultFromDb = true;
        this.loading = true;
        this.openedMarker?.mapObject.closePopup();
        this.layers = [];
        FiberService.getNewestPoints(this.bounds.toBBoxString())
        .then((response) => {
            this.fibers = response.data;
            this.mapFibersToLayer();
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            this.loading = false;
        });
    }

    public getHistorique(fiber: FiberPointDTO) {
        if (!this.resultFromDb) {
            this.loadingHistory = true;
            FiberService.getHistorique(fiber.signature)
            .then((response) => {
                fiber.created = response.data.created;
                fiber.lastUpdated = response.data.lastUpdated;
                fiber.eligibilitesFtth = response.data.eligibilitesFtth.sort((a, b) => (a.batiment < b.batiment && a.etapeFtth < b.etapeFtth ? -1 : 1));
                fiber.eligibilitesFtth.forEach(f => f.strEtapeFtth = EtapeFtth[f.etapeFtth]);
            })
            .catch((errors) => {
                this.createToast({ color:ISnackbarColor.Error, message: errors });
            })
            .finally(() => {
                this.loadingHistory = false;
            });
        }
    }

    public centerUpdate(center: LatLng) {
        this.userLocation = center;
    }

    public clearData() {
        this.openedMarker?.mapObject.closePopup();
        this.fibers = [];
        this.layers = [];
    }

    public centerMapOnPoint(point: FiberPointDTO) {
        this.userLocation = new LatLng(point.y, point.x);
        this.getHistorique(point);
        const marker = this.$refs['marker_'+point.signature] as LMarker[];
        this.openedMarker = marker[0];
        this.openedMarker.mapObject.openPopup();
    }

    public setIcon(fiber: FiberPointDTO) {
        if (fiber.eligibilitesFtth.length > 0) {
            const icon = this.icons.filter(a => a.code === fiber.eligibilitesFtth[0].etapeFtth)[0]?.icon;
            if (icon === undefined) {
                fiber.iconUrl = this.blackIcon;
            }
            fiber.iconUrl = icon;
        } else {
            fiber.iconUrl = this.redIcon;
        }
        fiber.iconClassName = this.opacityWithElderness(fiber);
    }

    public showHideLayer(code: EtapeFtth) {
        const selectedLayer = this.layers.filter(l => l.name === code.toString())[0];
        selectedLayer.visible = !selectedLayer.visible;
    }

    public boundsUpdated (bounds: L.LatLngBounds) {
        this.bounds = bounds;
    }

    public mapFibersToLayer() {
        this.fibers.forEach(fiber => {
            this.setIcon(fiber);
            fiber.eligibilitesFtth.forEach(f => f.strEtapeFtth = EtapeFtth[f.etapeFtth]);
        });
        this.layers = [];
        Object.keys(EtapeFtth).forEach((value) => {
            this.layers.push(
                {
                    markers: this.fibers.filter(f => f.eligibilitesFtth[0]?.etapeFtth === Number(value)),
                    name: value.toString(),
                    visible: true,
                }
            );
        });
        const noDataLayer = this.layers.filter(l => l.name === EtapeFtth._.toString())[0];
        noDataLayer.markers = noDataLayer.markers.concat(
            this.fibers.filter(f => f.eligibilitesFtth.length === 0)
        );
        const debugLayer = this.layers.filter(l => l.name === EtapeFtth.UNKNOWN.toString())[0];
        debugLayer.markers = debugLayer.markers.concat(
            this.fibers.filter(f => f.eligibilitesFtth?.some(x => x.etapeFtth === EtapeFtth.DEBUG))
        );
    }

    public centerMapOnLocation() {
        navigator.geolocation.getCurrentPosition(
            position => {
                this.userLocation = new LatLng(position.coords.latitude, position.coords.longitude);
            }, 
            error => this.createToast({ message: error.message, color: ISnackbarColor.Error }),
            {
                enableHighAccuracy: true,
                timeout: 10000
            });
    }

    public opacityWithElderness(fiber: FiberPointDTO) {
        if (!this.recentResult) {
            return 'opacity-100';
        }
        let mostRecentDate = Math.max(...fiber.eligibilitesFtth.map(f => new Date(f.created).getTime()));
        if (fiber.eligibilitesFtth.length === 0) {
            mostRecentDate = new Date(fiber.created).getTime();
        }
        let result = 100;
        if (mostRecentDate >= this.date.getTime() - 1 * 86400000) {
            result = 100;
        } else if (mostRecentDate >= this.date.getTime() - 3 * 86400000) {
            result = 60;
        } else if (mostRecentDate >= this.date.getTime() - 6 * 86400000) {
            result = 50;
        } else {
            result = 15;
        }
        return 'opacity-' + result;
    }

    public get orderedIcons() {
        return this.icons.sort((a, b) => a.order - b.order);
    }

    public get mapHeight() {
        switch(this.$vuetify.breakpoint.name) {
            case 'xs':
            case 'md':
                return "50vh";
            default:
                return "75vh";
        }
    }
}