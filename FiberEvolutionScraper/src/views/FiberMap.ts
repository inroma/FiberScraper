import axios from 'axios';
import 'leaflet/dist/leaflet.css';
import L, { LatLng } from "leaflet";
import { LMap, LTileLayer, LMarker, LLayerGroup, LPopup, LControl, LControlLayers, LIcon } from "vue2-leaflet";
import Vue from 'vue';
import { Action } from 'vuex-class';
import { Component } from 'vue-property-decorator';
import { ToastStoreMethods } from '@/store/ToastStore';
import { ISnackbar, ISnackbarColor } from '@/models/SnackbarInterface';
import FiberPointDTO, { EtapeFtth } from '@/models/FiberPointDTO';

@Component({
    components: {
        'l-map': LMap,
        'l-tile-layer': LTileLayer,
        'l-marker': LMarker,
        'l-popup': LPopup,
        'l-control': LControl,
        'l-layer-group': LLayerGroup,
        'l-icon': LIcon,
        LControlLayers
    }
})
export default class FiberMapVue extends Vue {
    //#region Public Properties
    public loading = false;
    public loadingHistory = true;
    public map: null | LMap = null;
    public userLocation = new LatLng(45.76, 4.83);
    public fibers: FiberPointDTO[] = [];
    public fiberHistory: FiberPointDTO[] = [];
    public zoom = 11;
    public tileProviders = [
        {
          name: 'OpenStreetMap',
          visible: true,
          attribution:
            '&copy; <a target="_blank" href="http://osm.org/copyright">OpenStreetMap</a> contributors',
          url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
        },
        {
            name: 'CartoDBDark',
            visible: false,
            url: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
            subdomains: 'abcd',
            maxZoom: 20
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
    public defaultIcon = new L.Icon.Default({
        iconUrl: '/icons/marker-blue.png',
        shadowUrl: ''
    });
    public redIcon = new L.Icon.Default({
        iconUrl: '/icons/marker-red.png',
        shadowUrl: '',
    });
    public greenIcon = new L.Icon.Default({
        iconUrl: '/icons/marker-green.png',
        shadowUrl: '',
    });
    public purpleIcon = new L.Icon.Default({
        iconUrl: '/icons/marker-purple.png',
        shadowUrl: '',
    });
    public brownIcon = new L.Icon.Default({
        iconUrl: '/icons/marker-orange.png',
        shadowUrl: '',
    });
    public blackIcon = new L.Icon.Default({
        iconUrl: '/icons/marker-black.png',
        shadowUrl: '',
    });
    public blueInvertedIcon = new L.Icon.Default({
        iconUrl: '/icons/marker-white.png',
        shadowUrl: '',
    });
    public layers: { markers: FiberPointDTO[], visible: boolean, name: string }[] = [];

    public icons = [{code: EtapeFtth.ELLIGIBLE, title: "Élligible", icon: this.greenIcon},
        {code: EtapeFtth.EN_COURS_IMMEUBLE, title: "Déploiement Immeuble", icon: this.purpleIcon},
        {code: EtapeFtth.TERMINE_QUARTIER, title: "Quartier Terminé", icon: this.blueInvertedIcon},
        {code: EtapeFtth.EN_COURS_QUARTIER, title: "Déploiement Quartier", icon: this.defaultIcon},
        {code: EtapeFtth.PREVU_QUARTIER, title: "Quartier Programmé", icon: this.brownIcon},
        {code: EtapeFtth._, title: "Aucune donnée", icon: this.redIcon},
        {code: EtapeFtth.UNKNOWN, title: "Statut inconnu", icon: this.blackIcon}];

    public get headers() {
        return [
            { text: 'Adresse', value: 'libAdresse' },
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
        this.recentResult = false;
        this.loading = true;
        this.openedMarker?.mapObject.closePopup();
        this.layers = [];
        axios.get<FiberPointDTO[]>('https://localhost:5001/api/fiber/GetCloseArea',
            { headers: { 'Content-Type': 'application/json' },
            params: { coordX: this.userLocation.lat, coordY: this.userLocation.lng }})
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

    public getFibers() {
        this.recentResult = false;
        this.loading = true;
        this.openedMarker?.mapObject.closePopup();
        this.layers = [];
        axios.get<FiberPointDTO[]>('https://localhost:5001/api/fiber/GetWideArea',
            { headers: { 'Content-Type': 'application/json' },
            params: { coordX: this.userLocation.lat, coordY: this.userLocation.lng }})
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
        this.recentResult = false;
        this.loading = true;
        this.openedMarker?.mapObject.closePopup();
        this.layers = [];
        axios.get<FiberPointDTO[]>('https://localhost:5001/api/fiber/GetFibers',
            { headers: { 'Content-Type': 'application/json' },
            params: { coordX: this.userLocation.lat, coordY: this.userLocation.lng }})
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
        this.recentResult = true;
        this.loading = true;
        this.openedMarker?.mapObject.closePopup();
        this.layers = [];
        axios.get<FiberPointDTO[]>('https://localhost:5001/api/fiber/GetNewestPoints',
            { headers: { 'Content-Type': 'application/json' },
            params: { data: this.bounds.toBBoxString() }})
        .then((response) => {
            this.fibers = response.data;
            this.mapFibersToLayer();
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            this.loading = false;
            this.recentResult = true;
        });
    }

    public getHistorique(fiber: FiberPointDTO) {
        this.loadingHistory = true;
        this.fiberHistory = [fiber];
        axios.post<FiberPointDTO[]>('https://localhost:5001/api/fiber/GetSameSignaturePoints', fiber, 
            { headers: { 'Content-Type': 'application/json' }})
        .then((response) => {
            this.fiberHistory.unshift(...response.data);
        })
        .catch((errors) => {
            this.createToast({ color:ISnackbarColor.Error, message: errors });
        })
        .finally(() => {
            this.loadingHistory = false;
        });
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

    public getIcon(fiber: FiberPointDTO): L.Icon.Default {
        const icon = this.icons.filter(a => a.code === fiber.etapeFtth)[0]?.icon;
        if (icon === undefined) {
            return this.blackIcon;
        }
        return icon;
    }

    public isEligibleToFTTH(item: FiberPointDTO) {
        return item.etapeFtth === EtapeFtth.ELLIGIBLE ? 'Oui' : 'Non';
    }

    public getEtapeFtthValue(etapeFtth: EtapeFtth) {
        return EtapeFtth[etapeFtth];
    }

    public showHideLayer(code: EtapeFtth) {
        const selectedLayer = this.layers.filter(l => l.name === EtapeFtth[code])[0];
        selectedLayer.visible = !selectedLayer.visible;
    }

    public currentLayer(code: EtapeFtth) {
        return this.layers.filter(l => l.name === EtapeFtth[code])[0];
    }

    public boundsUpdated (bounds: L.LatLngBounds) {
        this.bounds = bounds;
    }

    public mapFibersToLayer() {
        this.layers = [];
        Object.values(EtapeFtth).forEach((value) => {
            if (typeof(value) !== typeof(EtapeFtth)) {
                this.layers.push(
                    {
                        markers: this.fibers.filter(f => EtapeFtth[f.etapeFtth] === value),
                        name: value.toString(),
                        visible: true,
                    }
                );
            }
        });
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
        const ratio = (new Date(fiber.created).getDay()) / this.date.getDay() * 100;
        let result = 100;
        switch (true) {
            case 100 >= ratio && ratio > 90:
                result = 100
                break;
            case 90 >= ratio && ratio > 80:
                result = 90
                break;
            case 80 >= ratio && ratio > 70:
                result = 80
                break;
            case 70 >= ratio && ratio > 60:
                result = 70
                break;
            case 60 >= ratio && ratio > 50:
                result = 60
                break;  
            case 50 >= ratio && ratio > 40:
                result = 50
                break;
            default:
                result = 40
                break;
        }
        return 'opacity-' + result;
    }
}