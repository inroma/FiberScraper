import axios from 'axios';
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
import EligibiliteFtth from '@/models/EligibiliteFtthDTO';

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
    public defaultIcon = '/icons/marker-blue.png';
    public redIcon = '/icons/marker-red.png';
    public greenIcon = '/icons/marker-green.png';
    public purpleIcon = '/icons/marker-purple.png';
    public brownIcon = '/icons/marker-orange.png';
    public blackIcon = '/icons/marker-black.png';
    public blueInvertedIcon = '/icons/marker-white.png';
    public layers: { markers: FiberPointDTO[], visible: boolean, name: string }[] = [];

    public icons = [{code: EtapeFtth.ELLIGIBLE, title: "Élligible", icon: this.greenIcon},
        {code: EtapeFtth.EN_COURS_IMMEUBLE, title: "Déploiement Immeuble", icon: this.purpleIcon},
        {code: EtapeFtth.TERMINE_QUARTIER, title: "Quartier Terminé", icon: this.blueInvertedIcon},
        {code: EtapeFtth.EN_COURS_QUARTIER, title: "Déploiement Quartier", icon: this.defaultIcon},
        {code: EtapeFtth.PREVU_QUARTIER, title: "Quartier Programmé", icon: this.brownIcon},
        {code: EtapeFtth._, title: "Aucune donnée", icon: this.redIcon},
        {code: EtapeFtth.UNKNOWN, title: "Statut inconnu", icon: this.blackIcon}
    ];
    EtapeFtth = EtapeFtth;

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

    public updateFibers() {
        axios.get<number>('https://localhost:5001/api/fiber/UpdateWideArea',
            { headers: { 'Content-Type': 'application/json' },
            params: { coordX: this.userLocation.lat, coordY: this.userLocation.lng }})
        .then((response) => {
            this.createToast({ message: response.data + " points créés / mis à jour." });
        })
        .catch((errors) => {
            this.createToast({ color: ISnackbarColor.Error, message: errors });
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
        axios.get<FiberPointDTO>('https://localhost:5001/api/fiber/GetSameSignaturePoints', 
            { headers: { 'Content-Type': 'application/json' }, params: { signature: fiber.signature }})
        .then((response) => {
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
            const icon = this.icons.filter(a => a.code === Math.min(...fiber.eligibilitesFtth?.map(x => x.etapeFtth)))[0]?.icon;
            if (icon === undefined) {
                fiber.iconUrl = this.blackIcon;
            }
            fiber.iconUrl = icon;
        } else {
            fiber.iconUrl = this.redIcon;
        }
        fiber.iconClassName = this.opacityWithElderness(fiber);
    }

    public getEtapeFtthValue(eligibiliteFtth: EligibiliteFtth) {
        if (eligibiliteFtth !== null){
            return EtapeFtth[eligibiliteFtth.etapeFtth];
        } else {
            return EtapeFtth._;
        }
    }

    public showHideLayer(code: EtapeFtth) {
        const selectedLayer = this.layers.filter(l => l.name === code.toString())[0];
        selectedLayer.visible = !selectedLayer.visible;
    }

    public currentLayer(code: EtapeFtth) {
        return this.layers.filter(l => l.name === EtapeFtth[code])[0];
    }

    public boundsUpdated (bounds: L.LatLngBounds) {
        this.bounds = bounds;
    }

    public mapFibersToLayer() {
        this.fibers.forEach(f => this.setIcon(f));
        this.fibers.forEach(fiber => fiber.eligibilitesFtth.forEach(f => f.strEtapeFtth = EtapeFtth[f.etapeFtth]));
        this.layers = [];
        Object.keys(EtapeFtth).forEach((value) => {
            this.layers.push(
                {
                    markers: this.fibers.filter(f => Math.min(...f.eligibilitesFtth?.map(x => x.etapeFtth)) === Number(value)),
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
            this.fibers.filter(f => Math.min(...f.eligibilitesFtth?.map(x => x.etapeFtth)) === EtapeFtth.DEBUG)
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
}